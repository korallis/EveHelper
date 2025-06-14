import { useState } from "react";
import reactLogo from "./assets/react.svg";
import { invoke } from "@tauri-apps/api/core";
import "./App.css";
import { useEulaAcceptance } from './hooks/useEulaAcceptance';
import EulaModal from './components/EulaModal';

const App: React.FC = () => {
  const [greetMsg, setGreetMsg] = useState("");
  const [name, setName] = useState("");
  const { isAccepted, loading, error, acceptEula, declineEula } = useEulaAcceptance();

  async function greet() {
    // Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
    setGreetMsg(await invoke("greet", { name }));
  }

  // Show loading or error while checking EULA
  if (loading) return <div>Loading...</div>;
  if (error) return <div className="text-red-600">{error}</div>;

  // If EULA not accepted, show modal and block app
  if (isAccepted === false) {
    return (
      <EulaModal
        onAccept={acceptEula}
        onDecline={() => {
          declineEula();
          // Optionally: window.close() or show a blocking message
        }}
      />
    );
  }

  // If EULA declined, block app (should only show if isAccepted is exactly false)
  // This block is unreachable due to the previous check, so it can be removed or refactored if needed.

  return (
    <main className="container">
      <h1>Welcome to Tauri + React</h1>

      <div className="row">
        <a href="https://vitejs.dev" target="_blank">
          <img src="/vite.svg" className="logo vite" alt="Vite logo" />
        </a>
        <a href="https://tauri.app" target="_blank">
          <img src="/tauri.svg" className="logo tauri" alt="Tauri logo" />
        </a>
        <a href="https://reactjs.org" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <p>Click on the Tauri, Vite, and React logos to learn more.</p>

      <form
        className="row"
        onSubmit={(e) => {
          e.preventDefault();
          greet();
        }}
      >
        <input
          id="greet-input"
          onChange={(e) => setName(e.currentTarget.value)}
          placeholder="Enter a name..."
        />
        <button type="submit">Greet</button>
      </form>
      <p>{greetMsg}</p>
    </main>
  );
};

export default App;
