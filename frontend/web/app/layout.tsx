import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "MaliNovation",
  description: "Rethinking finance. Connecting possibilities.",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body style={{ margin: 0, fontFamily: "Arial, sans-serif" }}>
        {children}
      </body>
    </html>
  );
}
