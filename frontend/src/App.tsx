import { useState } from "react";
import { runAgent } from "./api";
import type { AgentResponse, AgentResult, Product } from "./types";

function App() {
  const [prompt, setPrompt] = useState(
    "Go to emag.ro and search for a laptop with 12GB RAM Core i5 and add it to my basket."
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [response, setResponse] = useState<AgentResponse | null>(null);

  const handleRun = async () => {
    setLoading(true);
    setError(null);
    setResponse(null);

    try {
      console.log("Running agent with prompt:", prompt);
      const data = await runAgent(prompt);
      setResponse(data);
    } catch (err: any) {
      setError(err.message ?? "Unknown error");
    } finally {
      setLoading(false);
    }
  };

  const renderToolResult = (result: AgentResult, index: number) => {
    if (result.tool === "searchProducts" && result.products) {
      return (
        <div key={index} style={{ marginBottom: "1rem" }}>
          <h3>Search results</h3>
          {result.products.length === 0 && <p>No products found.</p>}
          <ul>
            {result.products.map((p: Product) => (
              <li key={p.id}>
                <strong>{p.name}</strong> – {p.cpu}, {p.ramGb}GB RAM –{" "}
                {p.price} RON
              </li>
            ))}
          </ul>
        </div>
      );
    }

    if (result.tool === "addToBasket") {
      return (
        <div key={index} style={{ marginBottom: "1rem" }}>
          <h3>Basket updated</h3>
          <p>Product with ID {result.productId} was added to the basket.</p>
        </div>
      );
    }

    if (result.tool === "showBasket" && result.basket) {
      return (
        <div key={index} style={{ marginBottom: "1rem" }}>
          <h3>Basket</h3>
          {result.basket.length === 0 && <p>Basket is empty.</p>}
          <ul>
            {result.basket.map((item) => (
              <li key={item.id}>
                <strong>{item.name}</strong> × {item.quantity} – {item.price} RON
              </li>
            ))}
          </ul>
        </div>
      );
    }

    return (
      <div key={index}>
        <h3>Unknown tool result</h3>
        <pre>{JSON.stringify(result, null, 2)}</pre>
      </div>
    );
  };

  return (
    <div style={{ maxWidth: 800, margin: "2rem auto", fontFamily: "sans-serif" }}>
      <h1>AI Shopping Agent (Local Demo)</h1>
      <p>
        Type a natural language command, e.g.:<br />
        <code>
          Go to the local inmemory storage search for a laptop with 12GB RAM Core i5 and add it
          to my basket.
        </code>
      </p>

      <textarea
        style={{ width: "100%", height: 100, marginBottom: "1rem" }}
        value={prompt}
        onChange={(e) => setPrompt(e.target.value)}
      />

      <button onClick={handleRun} disabled={loading}>
        {loading ? "Thinking..." : "Run Agent"}
      </button>

      {error && (
        <div style={{ marginTop: "1rem", color: "red" }}>
          <strong>Error:</strong> {error}
        </div>
      )}

      {response && (
        <div style={{ marginTop: "2rem" }}>
          <h2>Agent Plan</h2>
          <pre>{JSON.stringify(response.plan, null, 2)}</pre>

          <h2>Execution Results</h2>
          {response.results.map((r, idx) => renderToolResult(r, idx))}
        </div>
      )}
    </div>
  );
}

export default App;
