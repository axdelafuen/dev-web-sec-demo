# OWASP A07:2021 Demo - Authentication Failures

Demo application illustrating authentication vulnerabilities and security best practices.

**For educational purposes only.** Contains intentional vulnerabilities. Never deploy the vulnerable version in production.

## Getting started

```bash
docker-compose up --build
```

**Access:**
- Frontend: http://localhost
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger

## Overview

This project demonstrates **OWASP A07:2021 - Identification and Authentication Failures** through two implementations:
- **Vulnerable version** - Shows common security flaws
- **Secure version** - Implements security best practices

## Vulnerable Version (`/api/vulnerable`)

**Demonstrated vulnerabilities:**
- No rate limiting → brute force attacks possible
- Detailed error messages → username enumeration
- No account lockout → unlimited attempts
- Weak password policy → accepts "123"
- Exposed logs → sensitive information visible

## Secure Version (`/api/secure`)

**Implemented protections:**
- Rate limiting (5 attempts / 5 min per IP)
- Account lockout (15 min after 5 failures)
- Generic error messages (no enumeration)
- Strong password policy (8+ chars, complexity required)
- Secure logging (anonymized)

**Password requirements:** Min 8 chars, uppercase, lowercase, digit, special character

## API Endpoints

### Vulnerable Endpoints
```
POST /api/vulnerable/register
POST /api/vulnerable/login
```

### Secure Endpoints
```
POST /api/secure/register
POST /api/secure/login
```

## Session vulnerabilty

Get a valid token, and paste in browser console:

```js
localStorage.setItem('authToken', "{validToken}");
localStorage.setItem('tokenExpiration', new Date(Date.now() + 86400000).toISOString());
```

## Architecture

**Backend:** .NET 9 + ASP.NET Core + PostgreSQL + JWT

**Frontend:** Angular 17 + TypeScript + nginx

**Infrastructure:** Docker + Docker Compose

## Resources

- [A07:2021 – Identification and Authentication Failures](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)
- [100k-most-used-passwords-NCSC.txt](https://github.com/danielmiessler/SecLists/blob/master/Passwords/Common-Credentials/100k-most-used-passwords-NCSC.txt)

## Author

[axdelafuen](https://github.com/axdelafuen)
