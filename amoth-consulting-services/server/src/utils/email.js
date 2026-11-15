import nodemailer from 'nodemailer';

export function assertEmailConfigured() {
  const { SMTP_HOST, SMTP_PORT, SMTP_FROM } = process.env;
  const port = Number(SMTP_PORT);
  if (!SMTP_HOST || !Number.isInteger(port) || port < 1 || port > 65535 || !SMTP_FROM ||
      Boolean(process.env.SMTP_USER) !== Boolean(process.env.SMTP_PASSWORD)) {
    const error = new Error('Password reset email is not configured. Check SMTP_HOST, SMTP_PORT, SMTP_FROM, and SMTP credentials.');
    error.code = 'EMAIL_CONFIGURATION';
    throw error;
  }
}

export async function sendPasswordResetEmail({ to, resetUrl }) {
  const { SMTP_HOST, SMTP_PORT, SMTP_USER, SMTP_PASSWORD, SMTP_FROM } = process.env;
  assertEmailConfigured();

  const transport = nodemailer.createTransport({
    host: SMTP_HOST,
    port: Number(SMTP_PORT),
    secure: Number(SMTP_PORT) === 465,
    ...(SMTP_USER && SMTP_PASSWORD ? { auth: { user: SMTP_USER, pass: SMTP_PASSWORD } } : {})
  });

  await transport.sendMail({
    from: SMTP_FROM,
    to,
    subject: 'Reset your Amoth Consulting password',
    text: `Use this link to reset your password. It expires in one hour: ${resetUrl}`,
    html: `<p>Use the link below to reset your password. It expires in one hour.</p><p><a href="${resetUrl.replaceAll('&', '&amp;')}">Reset password</a></p>`
  });
}
