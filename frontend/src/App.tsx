import { useState } from "react";
import { runAgent } from "./api";
import type { AgentResponse, AgentResult, Product } from "./types";
import "./App.css";

function App() {
  const [prompt, setPrompt] = useState(
    "Go to the local in-memory storage and search for a laptop with 12GB RAM Core i5 and add it to my basket."
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

    const LoadingBar = () => (
    <div className="loading-bar">
      <div className="loading-bar-fill" />
    </div>
  );

  const renderToolResult = (result: AgentResult, index: number) => {
    if (result.tool === "searchProducts" && result.products) {
      return (
        <div key={index} className="card card-section">
          <h3>Search results</h3>
          {result.products.length === 0 && <p>No products found.</p>}
          <ul className="list">
            {result.products.map((p: Product) => (
              <li key={p.id} className="list-item">
                <div className="product-main">
                  <span className="product-name">{p.name}</span>
                  <span className="product-price">{p.price} RON</span>
                </div>
                <div className="product-meta">
                  {p.cpu} · {p.ramGb}GB RAM
                </div>
              </li>
            ))}
          </ul>
        </div>
      );
    }

    if (result.tool === "addToBasket") {
      return (
        <div key={index} className="card card-section card-success">
          <h3>Basket updated</h3>
          <p>Product with ID {result.productId} was added to the basket.</p>
        </div>
      );
    }

    if (result.tool === "showBasket" && result.basket) {
      return (
        <div key={index} className="card card-section">
          <h3>Basket</h3>
          {result.basket.length === 0 && <p>Basket is empty.</p>}
          <ul className="list">
            {result.basket.map((item) => (
              <li key={item.id} className="list-item">
                <div className="product-main">
                  <span className="product-name">{item.name}</span>
                  <span className="product-price">{item.price} RON</span>
                </div>
                <div className="product-meta">Qty: {item.quantity}</div>
              </li>
            ))}
          </ul>
        </div>
      );
    }

    return (
      <div key={index} className="card card-section card-warning">
        <h3>Unknown tool result</h3>
        <pre className="code-block">{JSON.stringify(result, null, 2)}</pre>
      </div>
    );
  };

  return (
    <div className="app">
      <div className="app-shell">
        <header className="app-header">
          <div>
            <h1>AI Shopping Agent</h1>
            <p className="subtitle">Local demo powered by NET + React stack</p>
          </div>
          <span className="badge">Local Demo</span>
        </header>

        <section className="card input-card">
          <p className="hint">
            Try a natural language command, e.g.
            <br />
            <code className="inline-code">
              Go to the local in-memory storage, search for a laptop with 12GB RAM
              Core i5 and add it to my basket.
            </code>
          </p>

          <textarea
            className="prompt-input"
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
          />

          <div className="actions">
            <button
              className="btn primary"
              onClick={handleRun}
              disabled={loading}
            >
              {loading ? "Thinking..." : "Run Agent"}
            </button>
            {loading && <LoadingBar />}
          </div>

          {error && (
            <div className="alert alert-error">
              <strong>Error:</strong> {error}
            </div>
          )}
        </section>

        {response && (
          <section className="results-grid">
            <div className="card">
              <h2>Agent Plan</h2>
              <pre className="code-block">
                {JSON.stringify(response.plan, null, 2)}
              </pre>
            </div>

            <div className="card">
              <h2>Execution Results</h2>
              {response.results.length === 0 && (
                <p className="muted">No tool actions were executed.</p>
              )}
              {response.results.map((r, idx) => renderToolResult(r, idx))}
            </div>
          </section>
        )}
      </div>
    </div>
  );
}

export default App;
