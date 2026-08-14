import { NextRequest, NextResponse } from "next/server";

// UX-only: real access control lives entirely in the backend's [Authorize(Roles=...)] +
// service-layer ownership checks. This just avoids a flash of protected content / an
// obviously-wrong page by redirecting early, based on decoding (not verifying) the JWT.
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

const ROLE_HOME: Record<string, string> = {
  Admin: "/admin",
  Teacher: "/teacher",
  Student: "/student",
};

const PROTECTED_PREFIXES = ["/admin", "/teacher", "/student"];

function decodeRole(token: string): string | null {
  try {
    const payload = token.split(".")[1];
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const json = atob(normalized);
    const claims = JSON.parse(json) as Record<string, unknown>;
    const role = claims[ROLE_CLAIM];
    return typeof role === "string" ? role : null;
  } catch {
    return null;
  }
}

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get("auth_token")?.value;
  const role = token ? decodeRole(token) : null;

  const isProtected = PROTECTED_PREFIXES.some((prefix) => pathname.startsWith(prefix));

  if (isProtected && !token) {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  if (isProtected && role) {
    const homeForRole = ROLE_HOME[role];
    const matchesOwnSection = homeForRole && pathname.startsWith(homeForRole);
    if (!matchesOwnSection) {
      return NextResponse.redirect(new URL(homeForRole ?? "/login", request.url));
    }
  }

  if ((pathname === "/login" || pathname === "/") && role && ROLE_HOME[role]) {
    return NextResponse.redirect(new URL(ROLE_HOME[role], request.url));
  }

  if (pathname === "/") {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/teacher/:path*", "/student/:path*", "/login", "/"],
};
