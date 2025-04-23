import { ReactNode, useState } from "react";

interface ModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
}

interface ChatMessage {
  sender: string;
  content: string;
}

export default function Modal({ title, onClose, children }: ModalProps) {
  const [activeTab, setActiveTab] = useState("Renters");
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([
    { sender: "You", content: "Hey, I saw you booked this spot. Let me know if you have questions!" },
    { sender: "David", content: "Thank you! Just wondering if there’s a loading dock?" },
    { sender: "You", content: "Yup! There’s one at the rear entrance FAM." },
  ]);
  const [newMessage, setNewMessage] = useState("");

  const handleSend = () => {
    if (newMessage.trim() === "") return;
    setChatMessages([...chatMessages, { sender: "You", content: newMessage }]);
    setNewMessage("");
  };

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
              onClick={() => setActiveTab(tab)}
            >
              {tab}
            </button>
          ))}
        </div>

        <div className="p-4">
          {activeTab === "Renters" && children}

          {activeTab === "Messages" && (
            <>
              <div className="border h-40 overflow-y-auto p-2 rounded text-sm bg-gray-50 space-y-1 mb-2">
                {chatMessages.map((msg, index) => (
                  <div key={index}>
                    <strong>{msg.sender}:</strong> {msg.content}
                  </div>
                ))}
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
                  onClick={handleSend}
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