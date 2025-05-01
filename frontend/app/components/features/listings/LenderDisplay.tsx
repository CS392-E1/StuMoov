import React, { useState, useEffect } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { StorageLocation } from "@/types/storage";
import { Message, Session } from "@/types/chat";
import { Booking } from "@/types/booking";
import { Image } from "@/types/image";
import { PaymentStatus } from "@/types/payment";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { toast } from "sonner";
import ImageUpload from "./ImageUpload";

import {
  getMySessions,
  getMessagesBySessionId,
  sendMessage,
  getBookingsByStorageLocationId,
  uploadDropoffImage,
  getImagesByBookingId,
  confirmBooking,
} from "@/lib/api";

// Sorry if this is really messy, I should have split this into multiple components
// This might the worst code mankind has ever seen, please be warned

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
            {listing.storageLength != null &&
            listing.storageWidth != null &&
            listing.storageHeight != null
              ? `${listing.storageLength} x ${listing.storageWidth} x ${listing.storageHeight}`
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
      <div className="pt-4">
        <Button onClick={handleEdit} variant="outline">
          Edit Listing
        </Button>
      </div>
    </div>
  );
};

const MessagesTabContent = ({
  lenderId,
  listingId,
}: {
  lenderId: string;
  listingId: string;
}) => {
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
          // Filter sessions to only include those matching the current listingId
          const filteredSessions = response.data.data.filter(
            (session) => session.storageLocationId === listingId
          );
          setSessions(filteredSessions);
          // Automatically select the first session if only one matches
          if (filteredSessions.length === 1) {
            setSelectedSessionId(filteredSessions[0].id);
          }
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
  }, [lenderId, listingId]);

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
                  {session.renter?.displayName || "Unknown Renter"}
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
              Chat with {selectedSession?.renter?.displayName || "Renter"}
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
  const [bookingImages, setBookingImages] = useState<Record<string, Image[]>>(
    {}
  );
  const [confirmingBookingId, setConfirmingBookingId] = useState<string | null>(
    null
  );

  const fetchBookingsAndImages = async () => {
    setLoading(true);
    setError(null);
    setBookingImages({});
    try {
      const bookingResponse = await getBookingsByStorageLocationId(listingId);
      if (
        bookingResponse.status >= 200 &&
        bookingResponse.status < 300 &&
        bookingResponse.data.data
      ) {
        setBookings(bookingResponse.data.data);

        const imageFetchPromises = bookingResponse.data.data.map((booking) =>
          getImagesByBookingId(booking.id)
            .then((res) => ({
              bookingId: booking.id,
              images: res.data.data || [],
            }))
            .catch((err) => {
              console.warn(
                `Failed to fetch images for booking ${booking.id}:`,
                err
              );
              return { bookingId: booking.id, images: [] };
            })
        );

        const imageResults = await Promise.all(imageFetchPromises);

        const imagesMap: Record<string, Image[]> = {};
        imageResults.forEach((result) => {
          imagesMap[result.bookingId] = result.images;
        });
        setBookingImages(imagesMap);
      } else {
        throw new Error(
          bookingResponse.data.message || "Failed to fetch bookings"
        );
      }
    } catch (err: unknown) {
      console.error(`Failed to fetch bookings for listing ${listingId}:`, err);
      const errorMsg =
        err instanceof Error ? err.message : "Failed to load booking status.";
      setError(errorMsg);
      setBookings([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBookingsAndImages();
  }, [listingId]);

  const handleImageUploadSuccess = async (
    imageUrl: string,
    bookingId: string
  ) => {
    const toastId = toast.loading("Associating image with booking...");
    try {
      const response = await uploadDropoffImage(imageUrl, bookingId);
      if (response.status >= 200 && response.status < 300) {
        toast.success("Drop-off image uploaded and associated successfully!", {
          id: toastId,
        });
        const newImage = response.data.data as Image;
        if (newImage) {
          setBookingImages((prev) => ({
            ...prev,
            [bookingId]: [...(prev[bookingId] || []), newImage],
          }));
        } else {
          console.warn(
            "Backend did not return the new image object after upload."
          );
        }
      } else {
        throw new Error(response.data.message || "Failed to associate image");
      }
    } catch (error) {
      console.error("Failed to associate drop-off image:", error);
      const errorMsg = error instanceof Error ? error.message : "Unknown error";
      toast.error(`Failed to associate image: ${errorMsg}`, { id: toastId });
    }
  };

  const handleConfirmDropOff = async (bookingId: string) => {
    setConfirmingBookingId(bookingId);
    const toastId = toast.loading("Confirming booking and sending invoice...");
    try {
      const response = await confirmBooking(bookingId);
      if (
        response.status >= 200 &&
        response.status < 300 &&
        response.data.data
      ) {
        toast.success("Booking confirmed and invoice sent!", { id: toastId });
        fetchBookingsAndImages();
      } else {
        throw new Error(response.data.message || "Failed to confirm booking");
      }
    } catch (error) {
      console.error("Failed to confirm booking:", error);
      const errorMsg = error instanceof Error ? error.message : "Unknown error";
      toast.error(`Failed to confirm booking: ${errorMsg}`, { id: toastId });
    } finally {
      setConfirmingBookingId(null);
    }
  };

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
                className="border-b pb-3 last:border-b-0 last:pb-0 flex flex-col min-h-[250px]"
              >
                <div>
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
                    {typeof booking.totalPrice === "number"
                      ? (booking.totalPrice / 100).toFixed(2)
                      : "N/A"}
                  </p>
                </div>

                <div className="mt-2 border-t pt-2">
                  <p>
                    <strong>Payment Status:</strong>{" "}
                    {booking.payment ? (
                      <span
                        className={`font-medium ${
                          booking.payment.status === PaymentStatus.PAID
                            ? "text-green-600"
                            : booking.payment.status === PaymentStatus.OPEN ||
                              booking.payment.status === PaymentStatus.DRAFT
                            ? "text-yellow-600"
                            : "text-red-600"
                        }`}
                      >
                        {booking.payment.status}
                      </span>
                    ) : (
                      <span className="text-gray-500">N/A</span>
                    )}
                  </p>
                </div>

                {booking.status === "PENDING" && (
                  <div className="mt-auto pt-3 border-t">
                    {bookingImages[booking.id] &&
                    bookingImages[booking.id].length > 0 ? (
                      <div>
                        <h5 className="text-sm font-semibold mb-1">
                          Drop-off Image(s):
                        </h5>
                        {bookingImages[booking.id].map((image) => (
                          <img
                            key={image.id}
                            src={image.url}
                            alt={`Drop-off for booking ${booking.id}`}
                            className="mt-1 max-w-xs max-h-40 rounded border object-cover mb-2"
                          />
                        ))}
                        <Button
                          variant="outline"
                          size="sm"
                          className="mt-2"
                          onClick={() => handleConfirmDropOff(booking.id)}
                          disabled={confirmingBookingId === booking.id}
                        >
                          {confirmingBookingId === booking.id
                            ? "Confirming..."
                            : "Confirm Drop-Off"}
                        </Button>
                      </div>
                    ) : (
                      <div>
                        <h5 className="text-sm font-semibold mb-1">
                          Upload Drop-off Image:
                        </h5>
                        <ImageUpload
                          onUploadSuccess={(imageUrl) =>
                            handleImageUploadSuccess(imageUrl, booking.id)
                          }
                        />
                      </div>
                    )}
                  </div>
                )}
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
        {currentUserId && (
          <MessagesTabContent lenderId={currentUserId} listingId={listing.id} />
        )}
      </TabsContent>
      <TabsContent value="status" className="mt-4">
        <StatusTabContent listingId={listing.id} />
      </TabsContent>
    </Tabs>
  );
};
