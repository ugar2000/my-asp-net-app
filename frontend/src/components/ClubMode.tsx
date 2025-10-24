"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { SIGNALR_CLUB_HUB } from "@/lib/config";
import { ClubSessionStateDto } from "@/types/club";

export function ClubMode() {
  const [sessionId, setSessionId] = useState("weekly-lab");
  const [displayName, setDisplayName] = useState("Lead");
  const [document, setDocument] = useState("// написати код тут\n");
  const [consoleOutput, setConsoleOutput] = useState<string>("");
  const [participants, setParticipants] = useState<ClubSessionStateDto["participants"]>([]);
  const [status, setStatus] = useState("Очікування підключення…");
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const connect = useMemo(
    () =>
      async () => {
        if (connectionRef.current) {
          await connectionRef.current.stop();
        }
        const connection = new signalR.HubConnectionBuilder()
          .withUrl(SIGNALR_CLUB_HUB)
          .withAutomaticReconnect()
          .build();
        connection.on("SessionHydrated", (state: ClubSessionStateDto) => {
          setDocument(state.codeDocument);
          setConsoleOutput(state.consoleOutput);
          setParticipants(state.participants);
          setStatus("Сесію синхронізовано");
        });
        connection.on("SessionUpdated", (state: ClubSessionStateDto) => {
          setDocument(state.codeDocument);
          setConsoleOutput(state.consoleOutput);
          setParticipants(state.participants);
          setStatus("Оновлено");
        });
        await connection.start();
        connectionRef.current = connection;
        await connection.invoke("JoinSessionAsync", sessionId, displayName);
        setStatus("Підключено до сесії");
      },
    [sessionId, displayName],
  );

  const broadcast = async () => {
    const connection = connectionRef.current;
    if (!connection) return;
    await connection.invoke("PushUpdateAsync", {
      sessionId,
      editorDelta: JSON.stringify({ fullText: document }),
      outputAppend: null,
      author: displayName,
    });
  };

  useEffect(() => {
    connect();
    return () => {
      const connection = connectionRef.current;
      if (connection) {
        connection.stop();
      }
    };
  }, [connect]);

  return (
    <section className="card-panel p-10" id="club-mode">
      <div className="grid gap-6 md:grid-cols-[1fr,1.5fr]">
        <div>
          <p className="text-xs uppercase tracking-[0.4em] text-sky-300">Режим клубу</p>
          <h2 className="mt-3 text-3xl font-semibold text-sky-100">Спільне кодування з синхронізацією</h2>
          <p className="mt-3 text-sm text-slate-300">
            Лідер створює кімнату, учасники приєднуються та бачать спільний документ. SignalR синхронізує код та вивід між усіма учасниками.
          </p>
          <div className="mt-6 space-y-4">
            <div>
              <label className="text-sm text-slate-300">ID сесії</label>
              <input className="input-shell mt-1" value={sessionId} onChange={(e) => setSessionId(e.target.value)} />
            </div>
            <div>
              <label className="text-sm text-slate-300">Ім'я</label>
              <input className="input-shell mt-1" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
            </div>
            <div className="flex gap-3">
              <button className="button-primary" onClick={connect}>Приєднатись</button>
              <button className="button-secondary" onClick={broadcast}>Синхронізувати</button>
            </div>
            <p className="text-xs uppercase tracking-[0.35em] text-indigo-200">{status}</p>
          </div>
          <div className="mt-6 rounded-2xl border border-sky-400/20 bg-slate-900/70 p-4 text-sm text-slate-200">
            <p className="text-xs uppercase tracking-[0.4em] text-sky-300">Учасники</p>
            <ul className="mt-3 space-y-2">
              {participants.length === 0 && <li className="text-slate-400">Немає учасників</li>}
              {participants.map((p, idx) => (
                <li key={idx} className="flex items-center justify-between text-sm">
                  <span>{p.displayName}</span>
                  {p.isLeader && <span className="text-xs uppercase tracking-[0.35em] text-sky-300">Лідер</span>}
                </li>
              ))}
            </ul>
          </div>
        </div>
        <div className="space-y-4">
          <textarea
            className="input-shell h-60 font-mono text-sm"
            value={document}
            onChange={(e) => setDocument(e.target.value)}
          />
          <pre className="min-h-[8rem] whitespace-pre-wrap rounded-2xl border border-sky-400/30 bg-slate-950/80 p-4 text-xs text-slate-300">
            {consoleOutput || "Вивід сесії з'явиться тут."}
          </pre>
        </div>
      </div>
    </section>
  );
}
