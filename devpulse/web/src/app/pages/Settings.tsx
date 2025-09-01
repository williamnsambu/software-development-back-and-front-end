import { useState } from "react";
import { api } from "../lib/api";
import { toast } from "sonner";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../store/auth";

export default function Settings() {
  const [jiraEmail, setJiraEmail] = useState("");
  const [jiraToken, setJiraToken] = useState("");
  const [busy, setBusy] = useState(false);
  const clear = useAuthStore((s) => s.clear);
  const nav = useNavigate();

  async function connectJira(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      await api.post("/api/providers/jira/connect", { email: jiraEmail, apiToken: jiraToken });
      toast.success("Jira connected");
      setJiraToken("");
    } catch (err: any) {
      toast.error(err?.response?.data?.title ?? "Failed to connect Jira");
    } finally {
      setBusy(false);
    }
  }

  async function disconnect(provider: string) {
    setBusy(true);
    try {
      await api.delete(`/api/providers/${provider}`);
      toast.success(`${provider} disconnected`);
    } catch {
      toast.error(`Failed to disconnect ${provider}`);
    } finally {
      setBusy(false);
    }
  }

  async function syncNow() {
    setBusy(true);
    try {
      await api.post("/api/providers/sync");
      toast.success("Sync enqueued");
    } catch {
      toast.error("Failed to enqueue sync");
    } finally {
      setBusy(false);
    }
  }

  function signOut() {
    clear();
    nav("/login");
  }

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-8">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold">Settings</h1>
        <button onClick={signOut} className="px-3 py-2 rounded-lg border border-white/10 hover:bg-white/5">
          Sign out
        </button>
      </div>

      {/* Jira */}
      <section className="rounded-2xl border border-white/10 p-5 space-y-4 bg-white/5">
        <h2 className="font-medium">Jira</h2>
        <form onSubmit={connectJira} className="grid gap-3 sm:grid-cols-2">
          <input
            className="rounded-xl bg-black/20 border border-white/10 px-3 py-2"
            placeholder="Jira Email"
            type="email"
            value={jiraEmail}
            onChange={(e) => setJiraEmail(e.target.value)}
            required
          />
          <input
            className="rounded-xl bg-black/20 border border-white/10 px-3 py-2"
            placeholder="Jira API Token"
            type="password"
            value={jiraToken}
            onChange={(e) => setJiraToken(e.target.value)}
            required
          />
          <div className="sm:col-span-2 flex gap-3">
            <button
              disabled={busy}
              className="px-4 py-2 rounded-xl bg-white text-black disabled:opacity-60"
              type="submit"
            >
              {busy ? "…" : "Connect Jira"}
            </button>
            <button
              type="button"
              disabled={busy}
              onClick={() => disconnect("jira")}
              className="px-4 py-2 rounded-xl border border-white/20 hover:bg-white/5 disabled:opacity-60"
            >
              Disconnect
            </button>
          </div>
        </form>
      </section>

      {/* GitHub placeholder */}
      <section className="rounded-2xl border border-white/10 p-5 space-y-3 bg-white/5">
        <h2 className="font-medium">GitHub</h2>
        <div className="flex gap-3">
          <a
            href="/api/oauth/github/start?redirectUri=http://localhost:5173/settings&codeChallenge=dummy"
            className="px-4 py-2 rounded-xl bg-white text-black"
          >
            Connect (OAuth)
          </a>
          <button
            type="button"
            disabled={busy}
            onClick={() => disconnect("github")}
            className="px-4 py-2 rounded-xl border border-white/20 hover:bg-white/5 disabled:opacity-60"
          >
            Disconnect
          </button>
        </div>
        <p className="text-sm text-white/60">TODO: plug PKCE flow here.</p>
      </section>

      {/* Sync */}
      <section className="rounded-2xl border border-white/10 p-5 space-y-3 bg-white/5">
        <h2 className="font-medium">Data Sync</h2>
        <button
          disabled={busy}
          onClick={syncNow}
          className="px-4 py-2 rounded-xl bg-white text-black disabled:opacity-60"
        >
          {busy ? "…" : "Enqueue Sync Now"}
        </button>
        <p className="text-sm text-white/60">Background job is Quartz; see Program.cs schedule.</p>
      </section>
    </div>
  );
}