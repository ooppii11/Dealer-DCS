CREATE TABLE IF NOT EXISTS users(
	id TEXT PRIMARY KEY,
	user_name TEXT NOT NULL,
	password TEXT NOT NULL,
	email TEXT NOT NULL,
	phone_number TEXT,
);