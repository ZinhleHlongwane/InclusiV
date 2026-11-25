const capabilities = [
  ["Financial wellbeing", "Understand spending, plan budgets and build savings habits."],
  ["Insurance guidance", "Understand insurance terminology and approved information."],
  ["Payments and transactions", "Explore payment workflows using sandbox integrations."],
  ["Financial research", "Find relevant resources with source-backed explanations."],
];

export default function Home() {
  return (
    <main style={{ maxWidth: 960, margin: "64px auto", padding: "0 24px", color: "#172033" }}>
      <p style={{ color: "#53627a", fontWeight: 600 }}>MALINOVATION</p>
      <h1 style={{ fontSize: "clamp(2rem, 5vw, 3.5rem)", marginBottom: 12 }}>
        Rethinking finance. Connecting possibilities.
      </h1>
      <p style={{ maxWidth: 680, lineHeight: 1.7, color: "#53627a" }}>
        An independent project exploring how practical financial tools and coordinated specialist agents
        can help students and job seekers understand their finances and make informed decisions.
      </p>
      <section aria-label="Planned capabilities" style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: 16, marginTop: 40 }}>
        {capabilities.map(([title, description]) => (
          <article key={title} style={{ border: "1px solid #dce2eb", borderRadius: 12, padding: 20 }}>
            <h2 style={{ fontSize: 18 }}>{title}</h2>
            <p style={{ color: "#53627a", lineHeight: 1.6 }}>{description}</p>
            <small style={{ color: "#8a94a6" }}>Planned capability</small>
          </article>
        ))}
      </section>
      <p style={{ marginTop: 48, color: "#8a94a6", fontSize: 13 }}>
        Early foundation scaffold. These capabilities are not yet implemented in this web interface.
      </p>
    </main>
  );
}
