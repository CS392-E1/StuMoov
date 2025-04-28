import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { StorageLocation } from "@/types/storage";
import { LenderDisplay } from "@/components/features/listings/LenderDisplay";
import { RenterDisplay } from "@/components/features/listings/RenterDisplay";
import Calendar from "react-calendar";
import "react-calendar/dist/Calendar.css";

type ModalProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  listing: StorageLocation | null;
  isOwner: boolean;
  currentUserId: string | null;
};

export default function Modal({
  open,
  onOpenChange,
  listing,
  isOwner,
  currentUserId,
}: ModalProps) {
  const [showCalendar, setShowCalendar] = useState(false);
  const [dateRange, setDateRange] = useState<Date[] | null>(null);

  const handleConfirmBooking = () => {
    if (!dateRange || !dateRange[0] || !dateRange[1]) return;
    console.log("Booking confirmed from", dateRange[0].toDateString(), "to", dateRange[1].toDateString());
    setShowCalendar(false);
    setDateRange(null);
    onOpenChange(false);
  };

  if (!listing) return null;

  const title = isOwner
    ? `Manage Listing: ${listing.name}`
    : `Listing: ${listing.name}`;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-3xl md:max-w-4xl lg:max-w-5xl max-h-[90vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="p-4 overflow-y-auto flex-1 mt-2 border-t space-y-6">
          {isOwner ? (
            <LenderDisplay listing={listing} currentUserId={currentUserId} />
          ) : (
            <RenterDisplay listing={listing} currentUserId={currentUserId} />
          )}

          {!showCalendar ? (
            <div className="flex justify-center">
              <button
                onClick={() => setShowCalendar(true)}
                className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-2 rounded"
              >
                Select Drop-Off Dates
              </button>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-4">
              <Calendar
                selectRange
                minDate={new Date()}
                onChange={(range) => setDateRange(range as Date[] | null)}
                value={dateRange as Date[] | null}
                className="border rounded-lg p-2"
              />
              <button
                onClick={handleConfirmBooking}
                disabled={!dateRange || !dateRange[0] || !dateRange[1]}
                className="bg-green-600 hover:bg-green-700 text-white font-semibold px-6 py-2 rounded disabled:opacity-50"
              >
                Confirm Booking
              </button>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}