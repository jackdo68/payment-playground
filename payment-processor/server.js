import express from "express";
import pg from "pg";

const pool = new pg.Pool({
  connectionString:
    process.env.DATABASE_URL ??
    "postgres://payapp:devpass@localhost:5432/payapp",
});

const app = express();
app.use(express.json());

// Note the quoted "Accounts"/"Balance"/"UserId": EF Core created case-sensitive
// PascalCase identifiers (Topic 6), and unquoted names in Postgres fold to lowercase.

// Shared input check. In a real service you'd reach for zod — same idea as
// the DataAnnotations you're about to add on the .NET side.
function badInput({ userId, amount }) {
  if (!Number.isInteger(userId)) return "userId must be an integer";
  if (typeof amount !== "number" || !(amount > 0)) return "amount must be a positive number";
  return null;
}

app.post("/v1/withdraw", async (req, res) => {
  const err = badInput(req.body ?? {});
  if (err) return res.status(400).json({ error: err });
  const { userId, amount } = req.body;

  // THE line this service exists for: an ATOMIC conditional update.
  // Read-check-write happens inside the database as one indivisible statement —
  // no app-level lock, and it stays correct with any number of replicas.
  // This is the production-grade fix Topic 7 promised.
  const result = await pool.query(
    `UPDATE "Accounts" SET "Balance" = "Balance" - $1
     WHERE "UserId" = $2 AND "Balance" >= $1
     RETURNING "Balance"`,
    [amount, userId]
  );
  if (result.rowCount === 1) return res.json({ balance: result.rows[0].Balance });

  // 0 rows: either the account doesn't exist, or the balance guard failed.
  const exists = await pool.query(`SELECT 1 FROM "Accounts" WHERE "UserId" = $1`, [userId]);
  if (exists.rowCount === 0)
    return res.status(404).json({ error: `No account for user ${userId}` });
  return res.status(400).json({ error: "Insufficient funds" });
});

app.post("/v1/deposit", async (req, res) => {
  const err = badInput(req.body ?? {});
  if (err) return res.status(400).json({ error: err });
  const { userId, amount } = req.body;

  const result = await pool.query(
    `UPDATE "Accounts" SET "Balance" = "Balance" + $1
     WHERE "UserId" = $2
     RETURNING "Balance"`,
    [amount, userId]
  );
  if (result.rowCount === 0)
    return res.status(404).json({ error: `No account for user ${userId}` });
  return res.json({ balance: result.rows[0].Balance });
});

app.get("/healthz", (_req, res) => res.json({ status: "ok" }));

const port = process.env.PORT ?? 4000;
app.listen(port, () => console.log(`payment-processor listening on :${port}`));