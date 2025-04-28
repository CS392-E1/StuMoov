import { useState, useEffect } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { StorageLocation } from "@/types/storage";
import { Message } from "@/types/chat";
import { Booking, BookingStatus } from "@/types/booking";
import { Image } from "@/types/image";
import Calendar from "react-calendar";
import "react-calendar/dist/Calendar.css";
import { calculateBookingPrice } from "@/hooks/use-booking-price";
import { DateTime } from "luxon";
import { toast } from "sonner";
import {
  createSession,
  sendMessage,
  getBookingsByStorageLocationId,
  getMessagesBySessionId,
  getSessionByParticipants,
  createBooking,
  getImagesByBookingId,
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
  const [showCalendar, setShowCalendar] = useState(false);
  const [dateRange, setDateRange] = useState<CalendarRangeValue>(null);
  const [isBooking, setIsBooking] = useState(false);
  const [bookingImages, setBookingImages] = useState<Record<string, Image[]>>(
    {}
  );

  type CalendarRangeValue = [Date | null, Date | null] | null;

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

  const handleFetchBookings = async () => {
    setLoadingBookings(true);
    setBookingImages({});
    try {
      const res = await getBookingsByStorageLocationId(listing.id);
      if (res.status >= 200 && res.status < 300 && res.data.data) {
        const fetchedBookings = res.data.data;
        setBookings(fetchedBookings);

        const relevantBookings = fetchedBookings.filter(
          (b) => b.renterId === currentUserId
        );

        if (relevantBookings.length > 0) {
          const imageFetchPromises = relevantBookings.map((booking) =>
            getImagesByBookingId(booking.id)
              .then((imgRes) => ({
                bookingId: booking.id,
                images: imgRes.data.data || [],
              }))
              .catch((err) => {
                console.warn(
                  `RenterDisplay: Failed to fetch images for booking ${booking.id}:`,
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
        }
      } else {
        console.error("Failed to fetch bookings:", res.data.message);
        setBookings([]);
      }
    } catch (err) {
      console.error("Error fetching bookings:", err);
      setBookings([]);
    } finally {
      setLoadingBookings(false);
    }
  };

  useEffect(() => {
    if (listing.id) {
      handleFetchBookings();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [listing.id]);

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

  const isDateBooked = ({ date }: { date: Date }): boolean => {
    const currentDate = DateTime.fromJSDate(date).startOf("day");
    return bookings.some((booking) => {
      if (
        booking.status !== BookingStatus.PENDING &&
        booking.status !== BookingStatus.CONFIRMED
      ) {
        return false;
      }
      try {
        const startDate = DateTime.fromISO(booking.startDate).startOf("day");
        const endDate = DateTime.fromISO(booking.endDate).startOf("day");
        return currentDate >= startDate && currentDate <= endDate;
      } catch (e) {
        console.error("Error parsing booking dates:", e);
        return false; // Don't disable if dates are invalid
      }
    });
  };

  const handleConfirmBooking = async () => {
    if (
      !dateRange ||
      !dateRange[0] ||
      !dateRange[1] ||
      !currentUserId ||
      !listing
    )
      return;

    setIsBooking(true);

    const startDate = dateRange[0];
    const endDate = dateRange[1];
    // Calculate price in cents using the imported function
    const totalPriceInCents = calculateBookingPrice(
      startDate,
      endDate,
      listing.price ?? 0
    );

    // this is so butt ugly but it works
    const newBookingRequest: Omit<
      Booking,
      "id" | "createdAt" | "updatedAt" | "paymentId"
    > = {
      renterId: currentUserId,
      storageLocationId: listing.id,
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      totalPrice: totalPriceInCents,
      status: BookingStatus.PENDING, // Set status to PENDING
    };

    console.log("Creating booking request:", newBookingRequest);

    try {
      const response = await createBooking(newBookingRequest as Booking);

      if (
        response.status >= 200 &&
        response.status < 300 &&
        response.data.data
      ) {
        console.log("Booking created successfully:", response.data.data);
        setShowCalendar(false);
        setDateRange(null);
        toast.success("Booking request sent successfully!");
        handleFetchBookings();
      } else {
        throw new Error(
          response.data.message || `Failed with status ${response.status}`
        );
      }
    } catch (err) {
      console.error("Failed to create booking:", err);
      const errorMsg =
        err instanceof Error ? err.message : "An unknown error occurred.";
      toast.error(errorMsg);
    } finally {
      setIsBooking(false);
    }
  };

  return (
    <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
      <TabsList className="grid w-full grid-cols-3">
        <TabsTrigger value="details">Details</TabsTrigger>
        <TabsTrigger value="messages">Messages</TabsTrigger>
        <TabsTrigger value="status">Status</TabsTrigger>
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
          <div className="mt-6 border-t pt-4">
            {!showCalendar ? (
              <div className="flex justify-center">
                <Button
                  onClick={() => setShowCalendar(true)}
                  className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-2 rounded"
                >
                  Book Dates
                </Button>
              </div>
            ) : (
              <div className="flex flex-col items-center gap-4">
                <h4 className="font-semibold text-lg mb-2">
                  Select Booking Range
                </h4>
                <Calendar
                  selectRange
                  minDate={new Date()} // Prevent booking past dates
                  tileDisabled={isDateBooked}
                  onChange={(value) =>
                    setDateRange(value as CalendarRangeValue)
                  }
                  value={dateRange}
                  className="border rounded-lg p-2 shadow-sm"
                />
                <Button
                  onClick={handleConfirmBooking}
                  disabled={
                    !dateRange || !dateRange[0] || !dateRange[1] || isBooking
                  }
                  className="bg-green-600 hover:bg-green-700 text-white font-semibold px-6 py-2 rounded disabled:opacity-50 w-full sm:w-auto"
                >
                  {isBooking ? "Booking..." : "Request Booking"}
                </Button>
              </div>
            )}
          </div>
        </div>
      </TabsContent>

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
                    {typeof booking.totalPrice === "number"
                      ? (booking.totalPrice / 100).toFixed(2)
                      : "N/A"}
                  </p>
                  {bookingImages[booking.id] &&
                    bookingImages[booking.id].length > 0 && (
                      <div className="mt-2 pt-2 border-t">
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
                      </div>
                    )}
                </li>
              ))}
            </ul>
          </ScrollArea>
        )}
      </TabsContent>
    </Tabs>
  );
};
