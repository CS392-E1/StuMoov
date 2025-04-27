import React, { useState, useEffect } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { StorageLocation } from "@/types/storage";
import { Message, Session } from "@/types/chat";
import { Booking } from "@/types/booking";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";

import {
  getMySessions,
  getMessagesBySessionId,
  sendMessage,
  getBookingsByStorageLocationId,
} from "@/lib/api";

// Sorry if this is really messy, I should have split this into multiple components
const DetailsTabContent = ({ listing }: { listing: StorageLocation }) => {
  // TODO: Implement Edit functionality
  const handleEdit = () => {
    console.log("Edit listing:", listing.id);
  };

  return (
    <div className="space-y-4">
      <div>
        <h4 className="font-semibold text-lg mb-1">Description</h4>
        <p className="text-gray-700 dark:text-gray-300">
          {listing.description || "No description provided."}
        </p>
      </div>
      <div>
        <h4 className="font-semibold text-lg mb-1">Address</h4>
        <p className="text-gray-700 dark:text-gray-300">
          {listing.address || "No address provided."}
        </p>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <div>
          <h4 className="font-semibold text-lg mb-1">Dimensions</h4>
          <p className="text-gray-700 dark:text-gray-300">
            {listing.length && listing.width && listing.height
              ? `${listing.length} x ${listing.width} x ${listing.height}`
              : "Not specified"}
          </p>
        </div>
        <div>
          <h4 className="font-semibold text-lg mb-1">Price</h4>
          <p className="text-gray-700 dark:text-gray-300">
            {listing.price ? `$${listing.price.toFixed(2)}` : "Not specified"}
          </p>
        </div>
      </div>
      {/* TODO: Add available dates? Maybe a calendar component? */}
      <div className="pt-4">
        <Button onClick={handleEdit} variant="outline">
          Edit Listing
        </Button>
      </div>
    </div>
  );
};

const MessagesTabContent = ({ lenderId }: { lenderId: string }) => {
  const [sessions, setSessions] = useState<Session[]>([]);
  const [selectedSessionId, setSelectedSessionId] = useState<string | null>(
    null
  );
  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");
  const [loadingSessions, setLoadingSessions] = useState(true);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // TODO: Could cache this?
  // Fetch all sessions for the lender on mount
  useEffect(() => {
    const fetchSessions = async () => {
      setLoadingSessions(true);
      setError(null);
      try {
        const response = await getMySessions();
        if (
          response.status >= 200 &&
          response.status < 300 &&
          response.data.data
        ) {
          setSessions(response.data.data);
        } else {
          throw new Error(response.data.message || "Failed to fetch sessions");
        }
      } catch (err: unknown) {
        console.error("Failed to fetch chat sessions:", err);
        const errorMsg =
          err instanceof Error ? err.message : "Failed to load conversations.";
        setError(errorMsg);
        setSessions([]);
      } finally {
        setLoadingSessions(false);
      }
    };
    fetchSessions();
  }, []);

  // TODO: Could cache this?
  // Fetch messages when a session is selected
  useEffect(() => {
    if (!selectedSessionId) {
      setMessages([]);
      return;
    }
    const fetchMessages = async () => {
      setLoadingMessages(true);
      setError(null);
      try {
        const response = await getMessagesBySessionId(selectedSessionId);
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
          `Failed to fetch messages for session ${selectedSessionId}:`,
          err
        );
        const errorMsg =
          err instanceof Error ? err.message : "Failed to load messages.";
        setError(errorMsg);
        setMessages([]);
      } finally {
        setLoadingMessages(false);
      }
    };
    fetchMessages();
  }, [selectedSessionId]);

  const handleSelectSession = (sessionId: string) => {
    setSelectedSessionId(sessionId);
  };

  const handleSendMessage = async () => {
    if (!selectedSessionId || !newMessage.trim()) return;
    try {
      const response = await sendMessage(selectedSessionId, newMessage);
      if (
        response.status >= 200 &&
        response.status < 300 &&
        response.data.data
      ) {
        setMessages((prevMessages) => [...prevMessages, response.data.data!]);
      } else {
        console.warn(
          "Received non-success response or invalid message data from API after sending:",
          response.data.message
        );
        setError(response.data.message || "Failed to send message.");
      }
      setNewMessage("");
    } catch (err: unknown) {
      console.error("Failed to send message:", err);
      const errorMsg =
        err instanceof Error ? err.message : "Failed to send message.";
      setError(errorMsg);
    }
  };

  const selectedSession = sessions.find((s) => s.id === selectedSessionId);

  return (
    <div className="flex gap-4 h-[60vh]">
      <div className="w-1/3 border-r pr-4 flex flex-col">
        <h4 className="font-semibold mb-2 text-center">Conversations</h4>
        <ScrollArea className="flex-1">
          {loadingSessions && <p>Loading conversations...</p>}
          {error && !loadingSessions && (
            <p className="text-red-500 text-sm">{error}</p>
          )}
          {!loadingSessions && sessions.length === 0 && (
            <p>No conversations found.</p>
          )}
          <ul className="space-y-1">
            {sessions.map((session) => (
              <li key={session.id}>
                <button
                  onClick={() => handleSelectSession(session.id)}
                  className={`w-full text-left p-2 rounded hover:bg-gray-100 dark:hover:bg-gray-800 ${
                    selectedSessionId === session.id
                      ? "bg-gray-200 dark:bg-gray-700"
                      : ""
                  }`}
                >
                  {session.renterId || "Unknown Renter"}
                </button>
              </li>
            ))}
          </ul>
        </ScrollArea>
      </div>

      <div className="w-2/3 flex flex-col">
        {selectedSessionId ? (
          <>
            <h4 className="font-semibold mb-2">
              Chat with {selectedSession?.renterId || "Renter"}
            </h4>
            <ScrollArea className="border rounded p-2 flex-1 mb-2 bg-gray-50 dark:bg-gray-800">
              {loadingMessages && <p>Loading messages...</p>}
              {!loadingMessages && messages.length === 0 && (
                <p>No messages yet. Select a conversation.</p>
              )}
              <div className="space-y-2">
                {messages.map((msg) => (
                  <div
                    key={msg.id}
                    className={`text-sm ${
                      msg.senderId === lenderId ? "text-right" : "text-left"
                    }`}
                  >
                    <span
                      className={`inline-block p-2 rounded-lg ${
                        msg.senderId === lenderId
                          ? "bg-blue-500 text-white"
                          : "bg-gray-200 dark:bg-gray-600"
                      }`}
                    >
                      {msg.content}
                    </span>
                  </div>
                ))}
              </div>
            </ScrollArea>
            <div className="flex gap-2 mt-auto">
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
                disabled={!newMessage.trim() || loadingMessages}
              >
                Send
              </Button>
            </div>
          </>
        ) : (
          <div className="flex items-center justify-center h-full text-gray-500">
            Select a conversation to view messages.
          </div>
        )}
      </div>
    </div>
  );
};

const StatusTabContent = ({ listingId }: { listingId: string }) => {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchBookings = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await getBookingsByStorageLocationId(listingId);
        if (
          response.status >= 200 &&
          response.status < 300 &&
          response.data.data
        ) {
          setBookings(response.data.data);
        } else {
          throw new Error(response.data.message || "Failed to fetch bookings");
        }
      } catch (err: unknown) {
        console.error(
          `Failed to fetch bookings for listing ${listingId}:`,
          err
        );
        const errorMsg =
          err instanceof Error ? err.message : "Failed to load booking status.";
        setError(errorMsg);
        setBookings([]);
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, [listingId]);

  return (
    <div className="space-y-4">
      <h4 className="font-semibold text-lg mb-2">Booking Status</h4>
      {loading && <p>Loading status...</p>}
      {error && <p className="text-red-500 text-sm">Error: {error}</p>}
      {!loading && bookings.length === 0 && (
        <p>No current or past bookings found for this listing.</p>
      )}
      {!loading && bookings.length > 0 && (
        <ScrollArea className="h-[50vh] border rounded p-2">
          <ul className="space-y-3">
            {bookings.map((booking) => (
              <li
                key={booking.id}
                className="border-b pb-3 last:border-b-0 last:pb-0"
              >
                <p>
                  <strong>Renter:</strong>{" "}
                  {booking.renter?.displayName ||
                    booking.renter?.email ||
                    booking.renterId}
                </p>
                <p>
                  <strong>Dates:</strong>{" "}
                  {new Date(booking.startDate).toLocaleDateString()} -{" "}
                  {new Date(booking.endDate).toLocaleDateString()}
                </p>
                <p>
                  <strong>Status:</strong>{" "}
                  <span
                    className={`font-medium ${
                      booking.status === "CONFIRMED"
                        ? "text-green-600"
                        : booking.status === "PENDING"
                        ? "text-yellow-600"
                        : "text-red-600"
                    }`}
                  >
                    {booking.status}
                  </span>
                </p>
                <p>
                  <strong>Total Price:</strong> $
                  {booking.totalPrice?.toFixed(2) ?? "N/A"}
                </p>
                {/* TODO: Add Payment Status */}
              </li>
            ))}
          </ul>
        </ScrollArea>
      )}
    </div>
  );
};

type LenderDisplayProps = {
  listing: StorageLocation;
  currentUserId: string | null;
};

export const LenderDisplay: React.FC<LenderDisplayProps> = ({
  listing,
  currentUserId,
}) => {
  const [activeTab, setActiveTab] = useState("details");

  if (!currentUserId) {
    return <div>Error: User not identified.</div>;
  }
  if (listing.lenderId !== currentUserId) {
    console.error("LenderDisplay rendered for non-owner!");
    return <div>Error: Access denied.</div>;
  }

  return (
    <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
      <TabsList className="grid w-full grid-cols-3">
        <TabsTrigger value="details">Details</TabsTrigger>
        <TabsTrigger value="messages">Messages</TabsTrigger>
        <TabsTrigger value="status">Status</TabsTrigger>
      </TabsList>
      <TabsContent value="details" className="mt-4">
        <DetailsTabContent listing={listing} />
      </TabsContent>
      <TabsContent value="messages" className="mt-4">
        {currentUserId && <MessagesTabContent lenderId={currentUserId} />}
      </TabsContent>
      <TabsContent value="status" className="mt-4">
        <StatusTabContent listingId={listing.id} />
      </TabsContent>
    </Tabs>
  );
};
