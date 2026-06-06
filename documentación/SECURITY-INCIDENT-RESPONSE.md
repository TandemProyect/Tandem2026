# Security Incident Response (OpenAI + Azure PAT)

## 1) Immediate rotation

1. OpenAI:
   - Keep leaked key disabled.
   - Create a new key.
2. Azure DevOps:
   - Keep revoked PAT disabled.
   - Create a new PAT with minimum scopes.
3. Update local runtime secrets only:
   - `OPENAI_APIKEY`
   - `SENDGRID_APIKEY`
   - `AZDO_PAT`

## 2) Never hardcode secrets

- Use environment variables in scripts and local config files.
- Do not commit raw tokens in `.ps1`, `.md`, `.json`, `.config`.

## 3) Purge leaked secrets from Git history

Even after removing secrets from current files, old commits still expose them.

### Option A (recommended): `git filter-repo`

Install once (if missing):

```bash
pip install git-filter-repo
```

Create `replacements.txt` in repo root using the exact leaked values from the security emails:

```text
literal:<LEAKED_OPENAI_KEY_FROM_EMAIL>==>REDACTED_OPENAI_KEY
literal:<LEAKED_AZDO_PAT_FROM_EMAIL>==>REDACTED_AZDO_PAT
```

Run:

```bash
git filter-repo --replace-text replacements.txt
```

Force push rewritten history:

```bash
git push origin --force --all
git push origin --force --tags
```

Note: coordinate this with collaborators before force-push.

### Option B: rotate-only (temporary)

If history rewrite is not possible immediately:
- keep leaked tokens revoked,
- continue with new tokens,
- still schedule history rewrite later.

## 4) Verify no leaks remain

```bash
rg "sk-proj-|SENDGRID_APIKEY\" value=\"SG\\.|AZDO_PAT.*=" .
```

## 5) Add prevention

- Add pre-commit secret scanning (`gitleaks`).
- Add CI secret scanning on PRs.
- Keep on-demand token scopes minimal and short-lived.
