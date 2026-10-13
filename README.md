# 🛡️ ApexGateway

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![CI/CD Pipeline](https://github.com/Cristoff-dev/ApexGateway/actions/workflows/ci.yml/badge.svg)](https://github.com/Cristoff-dev/ApexGateway/actions/workflows/ci.yml)
[![YARP](https://img.shields.io/badge/YARP-Reverse_Proxy-blue)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

**ApexGateway** is an enterprise-grade, ultra-lightweight API Gateway engineered on **.NET 10** and **YARP (Yet Another Reverse Proxy)**. Designed specifically for modern cloud-native infrastructures, it serves as the critical ingress layer to secure, monitor, and heavily optimize egress traffic targeted at expensive third-party platforms (e.g., LLMs like OpenAI, Google Maps, and specialized SaaS).

The primary business objective of this infrastructure is **Egress Cost Optimization**. By implementing intelligent distributed caching at the edge, ApexGateway intercepts redundant payloads, reducing external API billing by up to 60% while maintaining a zero-trust security posture for backend credentials.

---

## 🏗️ Architecture & Request Lifecycle

The system processes inbound traffic through a highly optimized, allocation-free middleware pipeline, ensuring near-zero overhead before routing to external providers.

1. **Edge Ingress (HTTP/3):** Receives client multiplexed requests via HTTP/3 over QUIC, guaranteeing ultra-low latency and connection resilience.
2. **Traffic Control (Rate Limiting):** Evaluates fixed-window policies per IP. Malicious or abusive bursts are instantaneously rejected with `429 Too Many Requests`, protecting upstream quotas.
3. **Semantic-Ready Caching (Redis):** Hashes incoming request signatures against a Redis cluster. On a **Cache HIT**, the payload is returned in ~2ms, completely bypassing third-party billing.
4. **Zero-Trust Credential Masking:** On a **Cache MISS**, YARP Request Transforms dynamically inject highly privileged API keys natively at the server level. The client application remains fully agnostic of sensitive tokens.
5. **Observability & Telemetry:** Dispatches standard `System.Diagnostics.Metrics` (cache hit rates, latency histograms) ready for OpenTelemetry/Prometheus scraping.

---

## ✨ Core Capabilities

* 💰 **Cost-Optimized Edge Caching:** Native integration with `StackExchange.Redis` for lightning-fast distributed data retrieval.
* 🔒 **Security By Design (Secret Masking):** Safely holds and injects Bearer tokens and proprietary headers. Secrets never leak to frontend bundles or client devices.
* 🚦 **Native Resource Protection:** Built-in IP throttling and DDoS mitigation using modern ASP.NET Core Rate Limiting primitives.
* 📊 **Enterprise Observability:** Emits real-time operational metrics, allowing engineering teams to visualize cache effectiveness and actual dollars saved via Grafana dashboards.
* 🐳 **Rootless Cloud-Native Docker:** Packaged as an ultra-compact, multi-stage Alpine Linux container running under a non-root context, fully compliant with SOC 2 / ISO 27001 deployment standards.

---

## 🚀 Quick Start Guide

### Prerequisites
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* Docker Engine (for containerized Redis and application hosting)

### Local Environment Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/TU_USUARIO/ApexGateway.git
   cd ApexGateway
   ```

2. **Provision Local Cache:**
   Spin up a lightweight Redis container for local development:
   ```bash
   docker run -d --name redis-cache -p 6379:6379 redis:alpine
   ```

3. **Configure Environment Variables:**
   The Gateway requires upstream credentials injected safely at runtime:
   ```bash
   export OPENAI_API_KEY="sk-your-private-key-here"
   export ConnectionStrings__redis="localhost:6379"
   ```

4. **Build and Execute:**
   ```bash
   dotnet restore
   dotnet run --project src/ApexGateway.csproj -c Release
   ```
   *The Gateway is now actively listening and routing AI traffic securely at `/api/v1/ai/*`.*

---

## ⚙️ Continuous Integration (CI/CD)

Code quality and architectural integrity are enforced via **GitHub Actions**. 

Upon any push or pull request to the `main` branch, the automated pipeline:
1. Provisions an isolated Ubuntu runner.
2. Bootstraps the .NET 10 SDK ecosystem.
3. Discovers the solution file (`ApexGateway.sln`).
4. Executes the complete `xUnit` test suite leveraging `Moq` and `FluentAssertions` for strict dependency isolation. 

Deployments are strictly gated by successful test execution and compilation health.

---

## 📁 Repository Structure

```text
ApexGateway/
├── src/
│   ├── Config/           # DI extensions ensuring a clean Program.cs composition root
│   ├── Middlewares/      # High-performance pipeline interceptions (Telemetry, Redis Cache)
│   ├── Transforms/       # YARP request mutation logic for Zero-Trust Security Masking
│   ├── Program.cs        # Application bootstrapper and HTTP/3 Kestrel configuration
│   └── Dockerfile        # Highly optimized multi-stage build instructions for AWS/Render
├── tests/                # Comprehensive xUnit testing suite with fluent assertions
├── .github/workflows/    # CI/CD pipeline definitions
└── readme.md             # Advanced technical guidelines for cloud deployment
```

---
*Built with modern C# standards to drive efficiency, security, and performance at scale.*
