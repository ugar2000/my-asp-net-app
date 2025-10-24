import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { NewsFeed } from "@/components/NewsFeed";

export const dynamic = "force-dynamic";

export default function NewsPage() {
  return (
    <div className="bg-lab-gradient text-slate-100">
      <Header />
      <main className="mx-auto max-w-6xl px-6 py-16">
        <NewsFeed />
      </main>
      <Footer />
    </div>
  );
}
