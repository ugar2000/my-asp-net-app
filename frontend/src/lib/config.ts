export const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5050";
export const SIGNALR_ALGORITHM_HUB = `${API_BASE_URL}/hubs/algorithm`;
export const SIGNALR_CLUB_HUB = `${API_BASE_URL}/hubs/club`;
