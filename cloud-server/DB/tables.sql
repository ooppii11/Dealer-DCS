CREATE TABLE IF NOT EXISTS users(
    id SERIAL PRIMARY KEY,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    phone_number TEXT
);

CREATE TABLE IF NOT EXISTS file_metadata (
    id SERIAL PRIMARY KEY,
    creator_id int NOT NULL,
    name TEXT NOT NULL,
    size INT NOT NULL,
	type TEXT NOT NULL,
    creation_time TIMESTAMP DEFAULT NOW(),
	last_modify TIMESTAMP DEFAULT NOW(),

    FOREIGN KEY (creator_id) REFERENCES users(id) ON DELETE CASCADE
);
