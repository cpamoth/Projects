# AWS Security Setup

This application does not configure AWS account security from the codebase. Complete the following steps in the AWS console before production deployment.

## Root User

1. Sign in as the AWS account root user.
2. Open **IAM** -> **Dashboard** -> **Root user** -> **Enable MFA**.
3. Register at least one hardware or virtual MFA device and store the recovery details securely.
4. Do not create or use root access keys. Remove any existing root access keys.
5. Use the root user only for tasks that AWS requires, such as account or billing configuration.

## IAM Access

1. Do not share IAM users or credentials between people.
2. Prefer IAM Identity Center with federated workforce identities and MFA required for every user.
3. If IAM users are required, enable MFA on every human user and require it in permissions policies with `aws:MultiFactorAuthPresent`.
4. Use roles with short-lived credentials for deployments and workloads. Do not store IAM access keys in the repository, `.env` files, GitHub Actions variables, or application code.
5. Grant least-privilege permissions. Keep deployment permissions separate from database and production administration permissions.
6. Keep one documented, MFA-protected break-glass administrator account and monitor its use.

## Deployment Checklist

- Store application secrets such as `JWT_SECRET` and PostgreSQL credentials in AWS Secrets Manager or the hosting platform's secret store.
- Enable CloudTrail and send logs to protected storage.
- Enable GuardDuty and configure alerts for root or IAM activity.
- Configure billing and security alerts.
- Review IAM Access Analyzer findings and remove unused credentials.
- Confirm the production deployment identity cannot modify the root account or bypass MFA requirements.

These controls protect AWS administration. They do not add MFA to application users signing in through `/api/auth/login`; application-level MFA is a separate feature.