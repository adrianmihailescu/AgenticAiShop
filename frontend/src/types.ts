export interface Product {
  id: number;
  name: string;
  brand: string;
  cpu: string;
  ramGb: number;
  price: number;
  shop: string;
}

export interface BasketItem {
  id: number;
  name: string;
  quantity: number;
  price: number;
}

export interface AgentAction {
  type: string;
  query?: string;
  minRam?: number;
  cpu?: string;
  productId?: number;
}

export interface AgentPlan {
  actions: AgentAction[];
}

export interface AgentResult {
  tool: string;
  products?: Product[];
  basket?: BasketItem[];
  productId?: number;
}

export interface AgentResponse {
  plan: AgentPlan;
  results: AgentResult[];
}
