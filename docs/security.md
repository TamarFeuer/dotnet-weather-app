# Security posture — current state and planned hardening

## Identity: how the pipeline reaches Azure

- **What:** a dedicated **service principal** (`weatherapi-deploy`, a non-human "robot"
  identity in Entra ID), with the **Contributor** role scoped to the *Azure for Students*
  subscription. The pipeline deploys **as this principal**, never as a personal account.
- **Where:** the principal's credential (client secret) is stored **encrypted inside the
  Azure DevOps service connection** `azure-weatherapi-deploy`. It is never in git, never in
  the YAML. The pipeline reads it only when a step needs Azure (the Deploy stage).
- **Why this way:** automation gets its own least-privilege identity, so it can run
  unattended and a leaked credential is isolated and revocable without touching any human
  user.
- **Planned hardening:** replace the stored **secret** with **workload identity federation**
  — a *secretless* trust between Azure DevOps and Entra ID, so there is no client secret to
  store, leak, or rotate.

## Secrets: what exists and where each one lives

- **Service principal secret** — stored in the Azure DevOps **service connection**
  (encrypted). *Not in git.*
- **Database admin password** — supplied to the pipeline as a **secret pipeline variable**
  (`databasePassword`), injected into the Bicep deployment at runtime and baked into the App
  Service's connection string. *Not in git.*
- **GitHub token (`githubToken`)** — a fine-grained personal access token, scoped to only this
  repository with **Pull requests: read and write**, stored as a **secret pipeline variable**.
  The pipeline uses it to post the deployed URLs as a comment on the pull request. Least
  privilege caps the blast radius if it leaks. *Not in git.*
- **Storage account key** — **not stored anywhere**; the pipeline **fetches it at runtime**
  (`az storage account keys list`) and uses it only to upload the frontend, then discards it.
- **Local development DB password** — in `appsettings.Development.json`, which is
  **git-ignored**; a committed `*.example.json` template documents the shape without any real
  value.

**Principle throughout:** no real secret ever enters the repository. Local values stay on the
machine; CI/cloud values come from the environment (pipeline secret variables / service
connection).

## Access model: control plane vs. data plane

- The principal is a **Contributor**, which covers **control-plane** actions: create/deploy
  resources (resource group, PostgreSQL, App Service, Storage), deploy backend code, and
  *list* the storage account's keys.
- Contributor does **not** grant **data-plane** access to blobs, so the frontend **upload**
  authenticates with the **storage account key** (fetched via the allowed control-plane
  action) rather than the principal's identity.
- **Planned hardening:** grant the principal **Storage Blob Data Contributor** (a scoped
  data-plane role) so the upload uses its **own identity** and the **account key is dropped**
  entirely.

## Other planned hardening (security by design)

- **Key Vault:** move the database password (and any future secrets) into **Azure Key Vault**,
  referenced by the pipeline/app, instead of a raw pipeline variable.
- **Tighter role scope:** consider scoping the principal to a **resource group** instead of
  the whole subscription (least privilege), once the per-branch design allows it.
- **Database networking:** replace the `0.0.0.0` "allow all Azure services" firewall rule with
  **private networking / VNet integration**, so the database isn't reachable from arbitrary
  Azure services.
- **CORS:** the backend already restricts allowed origins to the known frontend URL (not `*`);
  keep this tight as environments change.

## One-line summary

> The pipeline authenticates to Azure as a least-privilege service principal whose secret is
> stored (encrypted) in an Azure DevOps service connection; no secret is ever committed to git.
> Current shortcuts — a stored client secret and a storage account key for blob upload — are
> known and each has a planned secretless replacement (workload identity federation and the
> Storage Blob Data Contributor role), tracked as security-by-design hardening.
