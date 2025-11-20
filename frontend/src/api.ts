import type { AgentResponse } from "./types";

const BASE_URL = "http://localhost:5297";

export async function runAgent(prompt: string): Promise<AgentResponse> {
    console.log("Sending prompt to agent:", prompt);
    console.log("API URL:", `${BASE_URL}/api/agent/run`);
    
  const res = await fetch(`${BASE_URL}/api/agent/run`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ prompt }),
  });

  if (!res.ok) {
    throw new Error(`Agent error: ${res.statusText}`);
  }

  return res.json();
}
