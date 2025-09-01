import axios, { AxiosError } from "axios";
import { useAuthStore } from "../store/auth";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let refreshing: Promise<void> | null = null;

async function doRefresh() {
  const { refreshToken, setTokens } = useAuthStore.getState();
  if (!refreshToken) throw new Error("no refresh token");
  const { data } = await api.post("/api/auth/refresh", { refreshToken });
  setTokens(data.accessToken, data.refreshToken);
}

api.interceptors.response.use(
  (res) => res,
  async (err: AxiosError) => {
    const original: any = err.config;
    if (err.response?.status === 401 && !original?._retry) {
      original._retry = true;
      try {
        refreshing = refreshing ?? doRefresh();
        await refreshing;
        refreshing = null;
        return api(original);
      } catch {
        useAuthStore.getState().clear();
      } finally {
        refreshing = null;
      }
    }
    return Promise.reject(err);
  }
);