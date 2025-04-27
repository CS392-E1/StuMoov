import { useState, useEffect } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { StorageLocation } from "@/types/storage";
import { Message } from "@/types/chat";
import { Booking } from "@/types/booking";
import {
  createSession,
  sendMessage,
  getBookingsByStorageLocationId,
  getMessagesBySessionId,
  getSessionByParticipants,
} from "@/lib/api";

type RenterDisplayProps = {
  listing: StorageLocation;
  currentUserId: string | null;
};

export const RenterDisplay: React.FC<RenterDisplayProps> = ({
  listing,
  currentUserId,
}) => {
  // All this logic could defnitely be asbtracted into seperate hooks and/or components
  // but we'll get to it if we have time

  const [activeTab, setActiveTab] = useState("details");
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loadingBookings, setLoadingBookings] = useState(false);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [messagesError, setMessagesError] = useState<string | null>(null);
  const [checkingSession, setCheckingSession] = useState(false);

  useEffect(() => {
    if (!sessionId) {
      setMessages([]);
      return;
    }

    const fetchMessages = async () => {
      setLoadingMessages(true);
      setMessagesError(null);
      try {
        const response = await getMessagesBySessionId(sessionId);
        if (
          response.status >= 200 &&
          response.status < 300 &&
          response.data.data
        ) {
          setMessages(response.data.data);
        } else {
          throw new Error(response.data.message || "Failed to fetch messages");
        }
      } catch (err: unknown) {
        console.error(
          `Failed to fetch messages for session ${sessionId}:`,
          err
        );
        const errorMsg =
          err instanceof Error ? err.message : "Failed to load messages.";
        setMessagesError(errorMsg);
        setMessages([]);
      } finally {
        setLoadingMessages(false);
      }
    };

    fetchMessages();
  }, [sessionId]);

  useEffect(() => {
    if (
      activeTab === "messages" &&
      !sessionId &&
      currentUserId &&
      listing.lenderId
    ) {
      const fetchSession = async () => {
        setCheckingSession(true);
        try {
          console.log(
            `Checking for existing session between Renter ${currentUserId} and Lender ${listing.lenderId}`
          );
          const response = await getSessionByParticipants(
            currentUserId,
            listing.lenderId,
            listing.id
          );
          if (response.status === 200 && response.data.data) {
            console.log("Existing session found:", response.data.data.id);
            setSessionId(response.data.data.id);
          } else if (response.status === 404) {
            console.log("No existing session found.");
            setMessages([]);
          } else {
            throw new Error(
              response.data.message || `Failed with status ${response.status}`
            );
          }
        } catch (err) {
          console.error("Error checking for existing session:", err);
        } finally {
          setCheckingSession(false);
        }
      };
      fetchSession();
    }
  }, [activeTab, sessionId, currentUserId, listing.lenderId, listing.id]);

  const handleSendMessage = async () => {
    if (!newMessage.trim() || !currentUserId || !listing.lenderId) return;

    try {
      let sid = sessionId;
      if (!sid) {
        console.log(
          "Creating session with:",
          currentUserId,
          listing.lenderId,
          listing.id
        );
        const sessionRes = await createSession(
          currentUserId,
          listing.lenderId,
          listing.id
        );
        if (
          sessionRes.status >= 200 &&
          sessionRes.status < 300 &&
          sessionRes.data.data
        ) {
          sid = sessionRes.data.data.id;
          setSessionId(sid);
        } else {
          throw new Error(
            sessionRes.data.message || "Failed to create chat session."
          );
        }
      }

      const messageRes = await sendMessage(sid, newMessage);
      if (
        messageRes.status >= 200 &&
        messageRes.status < 300 &&
        messageRes.data.data
      ) {
        const newMessageData = messageRes.data.data;
        if (newMessageData) {
          setMessages((prev) => [...prev, newMessageData]);
        }
      } else {
        throw new Error(messageRes.data.message || "Failed to send message.");
      }

      setNewMessage("");
    } catch (err) {
      console.error("Failed to send message:", err);
    }
  };

  const handleFetchBookings = async () => {
    setLoadingBookings(true);
    try {
      const res = await getBookingsByStorageLocationId(listing.id);
      if (res.status >= 200 && res.status < 300 && res.data.data) {
        setBookings(res.data.data);
      } else {
        console.error("Failed to fetch bookings:", res.data.message);
      }
    } catch (err) {
      console.error("Error fetching bookings:", err);
    } finally {
      setLoadingBookings(false);
    }
  };

  return (
    <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
      <TabsList className="grid w-full grid-cols-3">
        <TabsTrigger value="details">Details</TabsTrigger>
        <TabsTrigger value="messages">Messages</TabsTrigger>
        <TabsTrigger value="status" onClick={handleFetchBookings}>
          Status
        </TabsTrigger>
      </TabsList>

      {/* details tab */}
      <TabsContent value="details" className="mt-4">
        <div className="space-y-4">
          <h2 className="text-xl font-semibold">{listing.name}</h2>{" "}
          {/* Listing Title */}
          <p className="text-gray-600">{listing.description}</p>
          <div className="grid grid-cols-2 gap-4 pt-4">
            <div>
              <h4 className="font-semibold text-lg mb-1">Dimensions</h4>
              <p>
                {listing.storageLength &&
                listing.storageWidth &&
                listing.storageHeight
                  ? `${listing.storageLength} x ${listing.storageWidth} x ${listing.storageHeight} ft`
                  : "Not specified"}
              </p>
            </div>
            <div>
              <h4 className="font-semibold text-lg mb-1">Price</h4>
              <p>${(listing.price ?? 0).toFixed(2)} / month</p>
            </div>
          </div>
          <div>
            <h4 className="font-semibold text-lg mb-1">Address</h4>
            <p>{listing.address || "No address provided."}</p>
          </div>
        </div>
      </TabsContent>

      {/* messages tab */}
      <TabsContent value="messages" className="mt-4 flex flex-col h-[60vh]">
        <div className="flex-1 overflow-y-auto border rounded p-4 bg-gray-50">
          {loadingMessages && (
            <p className="text-gray-400">Loading messages...</p>
          )}
          {messagesError && (
            <p className="text-red-500">Error: {messagesError}</p>
          )}
          {!loadingMessages &&
          !messagesError &&
          !checkingSession &&
          messages.length === 0 ? (
            <p className="text-gray-400">
              Start a conversation with the lender!
            </p>
          ) : (
            <div className="space-y-2">
              {messages.map((msg) => (
                <div
                  key={msg.id}
                  className={`text-sm ${
                    msg.senderId === currentUserId ? "text-right" : "text-left"
                  }`}
                >
                  <span
                    className={`inline-block p-2 rounded-lg ${
                      msg.senderId === currentUserId
                        ? "bg-blue-500 text-white"
                        : "bg-gray-200 dark:bg-gray-600"
                    }`}
                  >
                    {msg.content}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
        <div className="flex gap-2 mt-2">
          <Input
            placeholder="Type your message..."
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            onKeyDown={(e) =>
              e.key === "Enter" && !e.shiftKey && handleSendMessage()
            }
          />
          <Button
            onClick={handleSendMessage}
            disabled={!newMessage.trim() || loadingMessages || checkingSession}
          >
            Send
          </Button>
        </div>
      </TabsContent>

      {/* status tab */}
      <TabsContent value="status" className="mt-4">
        <h4 className="font-semibold mb-2">Booking Status</h4>
        {loadingBookings ? (
          <p>Loading bookings...</p>
        ) : bookings.length === 0 ? (
          <p className="text-gray-400">This listing is currently unbooked.</p>
        ) : (
          <ScrollArea className="h-[40vh] border rounded p-2">
            <ul className="space-y-3">
              {bookings.map((booking) => (
                <li key={booking.id} className="border-b pb-2">
                  <p>
                    <strong>From:</strong>{" "}
                    {new Date(booking.startDate).toLocaleDateString()}{" "}
                    <strong>to</strong>{" "}
                    {new Date(booking.endDate).toLocaleDateString()}
                  </p>
                  <p>
                    <strong>Status:</strong>{" "}
                    {booking.status === "CONFIRMED" ? (
                      <span className="text-green-500">Confirmed</span>
                    ) : booking.status === "PENDING" ? (
                      <span className="text-yellow-500">Pending</span>
                    ) : booking.status === "CANCELLED" ? (
                      <span className="text-red-500">Cancelled</span>
                    ) : (
                      <span className="text-gray-500">Unknown</span>
                    )}
                  </p>
                  <p>
                    <strong>Total Price:</strong> $
                    {booking.totalPrice?.toFixed(2) ?? "N/A"}
                  </p>
                </li>
              ))}
            </ul>
          </ScrollArea>
        )}
      </TabsContent>
    </Tabs>
  );
};
