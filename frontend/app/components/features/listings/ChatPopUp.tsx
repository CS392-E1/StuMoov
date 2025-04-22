import axios from "axios";
import { useState, useEffect } from "react";

interface Message {
  id?: string;
  sender: string;
  receiver: string;
  content: string;
  timestamp?: string;
}

interface ChatPopupProps {
  receiver: string;
  onClose: () => void;
}

export const ChatPopup = ({ receiver, onClose }: ChatPopupProps) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");

  const fetchMessages = async () => {
    const res = await axios.get(`http://localhost:5004/api/message/${receiver}`);
    setMessages(res.data);
  };

  const sendMessage = async () => {
    if (newMessage.trim() === "") return;
    await axios.post("http://localhost:5004/api/message", {
      sender: "RenterUser", // hardcoded for now — ideally use auth
      receiver: receiver,
      content: newMessage,
    });
    setNewMessage("");
    fetchMessages();
  };

  useEffect(() => {
    fetchMessages();
  }, []);

  return (
    <div className="absolute top-24 right-4 bg-white p-4 rounded shadow-lg w-80 z-50">
      <h2 className="font-semibold mb-2">Chat with {receiver}</h2>
      <div className="border h-48 overflow-y-auto p-2 mb-2 rounded">
        {messages.map((msg) => (
          <div key={msg.id} className="mb-1">
            <strong>{msg.sender}:</strong> {msg.content}
          </div>
        ))}
      </div>
      <div className="flex gap-2">
        <input
          className="border p-1 flex-1 rounded"
          value={newMessage}
          onChange={(e) => setNewMessage(e.target.value)}
          placeholder="Type a message..."
        />
        <button
          className="bg-blue-600 text-white px-3 rounded"
          onClick={sendMessage}
        >
          Send
        </button>
      </div>
      <button className="text-sm text-gray-500 mt-2" onClick={onClose}>
        Close
      </button>
    </div>
  );
};
