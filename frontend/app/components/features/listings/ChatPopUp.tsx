import React from "react"; // <-- ADD THIS
import axios from "axios";
import { useState, useEffect } from "react";

export const ChatPopup = ({ receiver, onClose }: ChatPopupProps) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState("");

  // renter hardcoded for now
  const senderId = "11111111-2222-3333-4444-555555555555";

  const fetchMessages = async () => {
    if (!senderId || !receiver) return;
    const res = await axios.get(
      `http://localhost:5004/api/messages?user1=${senderId}&user2=${receiver}`
    );
    setMessages(res.data.data);
  };

  const sendMessage = async () => {
    if (newMessage.trim() === "") return;
    await axios.post("http://localhost:5004/api/messages", {
      senderId,
      recipientId: receiver,
      content: newMessage,
    });
    setNewMessage("");
    fetchMessages();
  };

  useEffect(() => {
    fetchMessages();
  }, [senderId, receiver]); // <-- IMPORTANT

  return (
    <div className="absolute top-24 right-4 bg-white p-4 rounded shadow-lg w-80 z-50">
      <h2 className="font-semibold mb-2">Chat</h2>
      <div className="border h-48 overflow-y-auto p-2 mb-2 rounded">
        {messages.map((msg) => (
          <div key={msg.id} className="mb-1">
            <strong>{msg.senderId === senderId ? "You" : "Lister"}:</strong>{" "}
            {msg.content}
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