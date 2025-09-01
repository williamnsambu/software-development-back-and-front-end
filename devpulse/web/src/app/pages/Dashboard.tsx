import { useQuery } from "@tanstack/react-query";
import { api } from "../lib/api";
import { useAuthStore } from "../store/auth";
import { Link } from "react-router-dom";
import { createPkce } from "../lib/pkce";

type PR = {
  repo: string;
  number: number;
  title: string;
  state: string;
  updatedAt: string;
  url: string;
  ciPassing: boolean;
};

type Issue = {
  repo: string;
  title: string;
  status: string;
  updatedAt: string;
  url: string;
};

type DashboardVmNormalized = {
  userId: string;
  repoCount: number;
  issueCount: number;
  prs: PR[];
  issues: Issue[];
  weather: number | null;
};

export default function Dashboard() {
  const token = useAuthStore((s) => s.accessToken);

  const q = useQuery({
    queryKey: ["dashboard"],
    queryFn: async (): Promise<DashboardVmNormalized> => {
      const { data } = await api.get<any>("/api/dashboard");
      return {
        userId: data.userId ?? data.UserId ?? "",
        repoCount: data.repoCount ?? data.RepoCount ?? 0,
        issueCount: data.issueCount ?? data.IssueCount ?? 0,
        prs: data.prs ?? data.PRs ?? [],
        issues: data.issues ?? data.Issues ?? [],
        weather: data.weather ?? data.Weather ?? null,
      };
    },
    enabled: !!token,          // don’t fetch until you’re logged in
    staleTime: 30_000,
  });

  async function connectGithub() {
    // 1) Create PKCE, store verifier (refreshing page will clear this)
    const { verifier, challenge } = await createPkce();
    sessionStorage.setItem("pkce_verifier", verifier);

    // 2) Ask API for authorization URL (sets state cookie)
    const redirectUri = `${window.location.origin}/oauth/callback`;
    const { data } = await api.get<{ authorizationUrl: string }>(
      "/api/oauth/github/start",
      { params: { redirectUri, codeChallenge: challenge } }
    );

    // 3) Redirect to GitHub
    window.location.href = data.authorizationUrl;
  }

  if (!token) {
    // If not logged in, nudge to login first
    return (
      <ScreenShell>
        <PageHeader onConnect={connectGithub} />
        <EmptyState title="Please log in first" subtitle="Use the Login page, then come back." />
      </ScreenShell>
    );
  }

  if (q.isLoading) {
    return (
      <ScreenShell>
        <PageHeader onConnect={connectGithub} />
        <Skeleton />
      </ScreenShell>
    );
  }

  if (q.isError || !q.data) {
    return (
      <ScreenShell>
        <PageHeader onConnect={connectGithub} />
        <EmptyState title="Failed to load" subtitle="Check API is running and token still valid." />
      </ScreenShell>
    );
  }

  const d = q.data;

  return (
    <ScreenShell>
      <PageHeader onConnect={connectGithub} />

      {/* Stats */}
      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard label="Repositories" value={d.repoCount} />
        <StatCard label="Open Issues" value={d.issueCount} />
        <StatCard
          label="Weather"
          value={d.weather != null ? `${Math.round(d.weather)}°C` : "—"}
          hint="(OpenWeather)"
        />
      </div>

      {/* PRs */}
      <Section title="Recent Pull Requests">
        {(d.prs?.length ?? 0) === 0 ? (
          <EmptyRows text="No recent PRs" />
        ) : (
          <div className="overflow-hidden rounded-xl border border-white/10">
            <table className="min-w-full text-sm">
              <thead className="bg-white/5 text-left text-xs uppercase tracking-wide">
                <tr>
                  <Th>Repo</Th>
                  <Th>#</Th>
                  <Th>Title</Th>
                  <Th>Status</Th>
                  <Th>CI</Th>
                  <Th>Updated</Th>
                  <Th></Th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/10">
                {d.prs!.map((pr) => (
                  <tr key={`${pr.repo}-${pr.number}`} className="hover:bg-white/5">
                    <Td className="font-medium">{pr.repo}</Td>
                    <Td>#{pr.number}</Td>
                    <Td className="max-w-[32rem] truncate">{pr.title}</Td>
                    <Td>
                      <Pill tone={pr.state === "open" ? "green" : pr.state === "merged" ? "purple" : "zinc"}>
                        {pr.state}
                      </Pill>
                    </Td>
                    <Td>
                      <Pill tone={pr.ciPassing ? "green" : "red"}>
                        {pr.ciPassing ? "passing" : "failing"}
                      </Pill>
                    </Td>
                    <Td>{formatTime(pr.updatedAt)}</Td>
                    <Td className="text-right">
                      <a
                        className="text-sky-400 hover:text-sky-300 underline underline-offset-4"
                        href={pr.url}
                        target="_blank"
                        rel="noreferrer"
                      >
                        Open
                      </a>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Section>

      {/* Issues */}
      <Section title="Recent Issues">
        {(d.issues?.length ?? 0) === 0 ? (
          <EmptyRows text="No recent issues" />
        ) : (
          <div className="overflow-hidden rounded-xl border border-white/10">
            <table className="min-w-full text-sm">
              <thead className="bg-white/5 text-left text-xs uppercase tracking-wide">
                <tr>
                  <Th>Repo</Th>
                  <Th>Title</Th>
                  <Th>Status</Th>
                  <Th>Updated</Th>
                  <Th></Th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/10">
                {d.issues!.map((it, idx) => (
                  <tr key={idx} className="hover:bg-white/5">
                    <Td className="font-medium">{it.repo}</Td>
                    <Td className="max-w-[40rem] truncate">{it.title}</Td>
                    <Td>
                      <Pill tone={it.status === "open" ? "yellow" : "zinc"}>{it.status}</Pill>
                    </Td>
                    <Td>{formatTime(it.updatedAt)}</Td>
                    <Td className="text-right">
                      <a
                        className="text-sky-400 hover:text-sky-300 underline underline-offset-4"
                        href={it.url}
                        target="_blank"
                        rel="noreferrer"
                      >
                        Open
                      </a>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Section>
    </ScreenShell>
  );
}

/* --- tiny UI bits --- */
function ScreenShell({ children }: { children: React.ReactNode }) {
  return <div className="mx-auto max-w-6xl p-6 space-y-8">{children}</div>;
}
function PageHeader({ onConnect }: { onConnect: () => void }) {
  return (
    <div className="flex items-center justify-between">
      <h1 className="text-2xl font-semibold">Dashboard</h1>
      <div className="space-x-2">
        <button
          onClick={onConnect}
          className="inline-flex items-center rounded-lg border border-white/10 px-3 py-1.5 text-sm hover:bg-white/5"
        >
          Connect GitHub
        </button>
        <Link
          to="/login"
          onClick={() => localStorage.clear()}
          className="inline-flex items-center rounded-lg border border-white/10 px-3 py-1.5 text-sm hover:bg-white/5"
        >
          Sign out
        </Link>
      </div>
    </div>
  );
}
function StatCard({ label, value, hint }: { label: string; value: React.ReactNode; hint?: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/[0.03] p-5">
      <div className="text-xs uppercase text-white/60">{label}</div>
      <div className="mt-2 text-2xl font-semibold">{value}</div>
      {hint && <div className="mt-1 text-xs text-white/40">{hint}</div>}
    </div>
  );
}
function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold">{title}</h2>
      {children}
    </section>
  );
}
function Th({ children }: { children: React.ReactNode }) {
  return <th className="px-3 py-2 text-white/60">{children}</th>;
}
function Td({ children, className = "" }: { children: React.ReactNode; className?: string }) {
  return <td className={`px-3 py-2 ${className}`}>{children}</td>;
}
function Pill({
  children,
  tone = "zinc",
}: {
  children: React.ReactNode;
  tone?: "green" | "yellow" | "red" | "purple" | "zinc";
}) {
  const map: Record<string, string> = {
    green: "bg-green-500/15 text-green-300 ring-1 ring-inset ring-green-500/20",
    yellow: "bg-yellow-500/15 text-yellow-300 ring-1 ring-inset ring-yellow-500/20",
    red: "bg-red-500/15 text-red-300 ring-1 ring-inset ring-red-500/20",
    purple: "bg-purple-500/15 text-purple-300 ring-1 ring-inset ring-purple-500/20",
    zinc: "bg-white/10 text-white/70 ring-1 ring-inset ring-white/15",
  };
  return (
    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs ${map[tone]}`}>
      {children}
    </span>
  );
}
function EmptyRows({ text }: { text: string }) {
  return (
    <div className="rounded-xl border border-white/10 bg-white/[0.02] p-6 text-center text-sm text-white/60">
      {text}
    </div>
  );
}
function EmptyState({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/[0.03] p-8 text-center">
      <div className="text-lg font-medium">{title}</div>
      {subtitle && <div className="mt-1 text-sm text-white/60">{subtitle}</div>}
    </div>
  );
}
function Skeleton() {
  return (
    <div className="space-y-4 animate-pulse">
      <div className="grid gap-4 sm:grid-cols-3">
        {[0, 1, 2].map((i) => (
          <div key={i} className="h-24 rounded-2xl bg-white/5" />
        ))}
      </div>
      <div className="h-48 rounded-xl bg-white/5" />
      <div className="h-48 rounded-xl bg-white/5" />
    </div>
  );
}
function formatTime(iso: string) {
  try {
    const d = new Date(iso);
    return d.toLocaleString();
  } catch {
    return iso;
  }
}