import type { Metadata } from "next";
export const metadata: Metadata = {
  title: "Financial Inclusion Platform",
  description: "Financial assistance and guidance for students and job seekers.",
};
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="en"><body style={{ margin: 0, fontFamily: "Arial, sans-serif" }}>{children}</body></html>;
}
