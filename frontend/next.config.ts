import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Emits .next/standalone: a self-contained server with only the traced dependencies, so the
  // Docker runtime image doesn't have to carry all of node_modules. See frontend/Dockerfile.
  output: "standalone",
};

export default nextConfig;
