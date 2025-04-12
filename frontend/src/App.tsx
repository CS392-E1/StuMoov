import './firebase';
import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import React from 'react';
import './App.tsx';
import './firebase';
import LoginForm from './components/loginform';

function App() {
  const [count, setCount] = useState(0)

  return (
    
    <>
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
      <h1 className="text-2xl font-bold mb-4">My Group Project App</h1>
      <LoginForm />
    </div>

      <div className="flex justify-center items-center gap-8 p-4">
        <a href="https://vite.dev" target="_blank" className="hover:opacity-80 transition-opacity">
          <img src={viteLogo} className="h-24 w-24" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank" className="hover:opacity-80 transition-opacity">
          <img src={reactLogo} className="h-24 w-24 animate-[spin_3s_linear_infinite]" alt="React logo" />
        </a>
      </div>
      <h1 className="text-4xl font-bold text-center my-6 text-gray-800">Vite + React</h1>
      <div className="max-w-md mx-auto bg-white p-6 rounded-lg shadow-md">
        <button 
          onClick={() => setCount((count) => count + 1)}
          className="w-full bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-4 rounded transition-colors mb-4"
        >
          count is {count}
        </button>
        <p className="text-gray-700 text-center">
          Edit <code className="bg-gray-100 px-1 py-0.5 rounded text-sm font-mono">src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className="text-gray-500 text-center mt-8 text-sm">
        Click on the Vite and React logos to learn more
      </p>
    </>
  )
}

export default App
