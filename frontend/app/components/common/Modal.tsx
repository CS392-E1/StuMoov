import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { StorageLocation } from "@/types/storage";
import { LenderDisplay } from "@/components/features/listings/LenderDisplay";
import { RenterDisplay } from "@/components/features/listings/RenterDisplay";

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

        <div className="p-4 overflow-y-auto flex-1 mt-2 border-t">
          {isOwner ? (
            <LenderDisplay listing={listing} currentUserId={currentUserId} />
          ) : (
            <RenterDisplay listing={listing} currentUserId={currentUserId} />
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
