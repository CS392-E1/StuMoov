import { ReactNode } from "react";
import { Message, User } from "@/types/storage";

interface ModalProps {
  title: string;
  onClose: () => void;
  activeTab: "Renters" | "Messages";
  setActiveTab: (tab: "Renters" | "Messages") => void;
  chatMessages: Message[];
  newMessage: string;
  setNewMessage: (msg: string) => void;
  onSendMessage: () => void;
  interestedRenters: User[];
  currentUserId: string;
}

export default function Modal({
  title,
  onClose,
  activeTab,
  setActiveTab,
  chatMessages,
  newMessage,
  setNewMessage,
  onSendMessage,
  interestedRenters,
  currentUserId,
}: ModalProps) {
  return (
    <div className="fixed inset-0 bg-gray-800 bg-opacity-30 backdrop-blur-sm flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl max-h-[80vh] overflow-y-auto">
        <div className="flex justify-between items-center p-4 border-b">
          <h2 className="text-xl font-semibold">{title}</h2>
          <button onClick={onClose} className="text-gray-600 hover:text-black text-lg">&times;</button>
        </div>

        <div className="border-b flex">
          {["Renters", "Messages"].map((tab) => (
            <button
              key={tab}
              className={`flex-1 p-2 text-sm font-medium transition ${
                activeTab === tab
                  ? "border-b-2 border-blue-600 text-blue-600"
                  : "text-gray-600 hover:text-blue-500"
              }`}
              onClick={() => setActiveTab(tab as "Renters" | "Messages")}
            >
              {tab}
            </button>
          ))}
        </div>

        <div className="p-4">
          {activeTab === "Renters" && (
            <ul className="space-y-2">
              {interestedRenters.length > 0 ? (
                interestedRenters.map((renter) => (
                  <li key={renter.id} className="p-2 border rounded">
                    <p><strong>Email:</strong> {renter.email}</p>
                    <p><strong>Display Name:</strong> {renter.displayName || "N/A"}</p>
                  </li>
                ))
              ) : (
                <p>No interested renters found.</p>
              )}
            </ul>
          )}

          {activeTab === "Messages" && (
            <>
              <div className="border h-40 overflow-y-auto p-2 rounded text-sm bg-gray-50 space-y-1 mb-2">
                {chatMessages.length > 0 ? (
                  chatMessages.map((msg) => (
                    <div key={msg.id}>
                      <strong>{msg.senderId === currentUserId ? "You" : "Renter"}:</strong> {msg.content}
                    </div>
                  ))
                ) : (
                  <p>No messages yet.</p>
                )}
              </div>
              <div className="flex gap-2">
                <input
                  className="border p-2 flex-1 rounded"
                  value={newMessage}
                  onChange={(e) => setNewMessage(e.target.value)}
                  placeholder="Type your message..."
                />
                <button
                  className="bg-blue-600 text-white px-4 rounded"
                  onClick={onSendMessage}
                >
                  Send
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}