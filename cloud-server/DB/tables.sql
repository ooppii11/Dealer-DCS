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


CREATE TABLE IF NOT EXISTS file_location (
    file_id TEXT NOT NULL,
    primary_server_ip TEXT NOT NULL,
    backup_server_ip_1 TEXT NOT NULL,
    backup_server_ip_2 TEXT NOT NULL,

    FOREIGN KEY (file_id) REFERENCES file_metadata(id) ON DELETE CASCADE
);
