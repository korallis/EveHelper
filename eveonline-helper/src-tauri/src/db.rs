//! Encrypted SQLite schema and access logic for EveOnline Helper
//
// Uses sqlx for async, type-safe DB access. For encryption, use sqlcipher if available.
//
// Tables:
// - users: stores EVE character/user info
// - skills: stores skill data for each user
// - ships: stores ship and fitting data
// - modules: stores module data for fits
//
// NOTE: In production, store the encryption key securely (e.g., OS keychain).

use sqlx::{SqlitePool, sqlite::SqliteConnectOptions, sqlite::SqliteJournalMode, sqlite::SqliteSynchronous, ConnectOptions};
use std::str::FromStr;

/// Initialize the encrypted SQLite database connection
pub async fn init_db(db_path: &str, encryption_key: &str) -> Result<SqlitePool, sqlx::Error> {
    let mut options = SqliteConnectOptions::from_str(db_path)?
        .journal_mode(SqliteJournalMode::Wal)
        .synchronous(SqliteSynchronous::Full)
        .create_if_missing(true);
    // Enable encryption if using sqlcipher
    options = options.pragma("key", encryption_key);
    SqlitePool::connect_with(options).await
}

/// Create tables for users, skills, ships, and modules
pub async fn create_tables(pool: &SqlitePool) -> Result<(), sqlx::Error> {
    // Users table
    sqlx::query(
        r#"CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY,
            character_id TEXT NOT NULL UNIQUE,
            name TEXT,
            last_login DATETIME
        )"#
    ).execute(pool).await?;
    // Skills table
    sqlx::query(
        r#"CREATE TABLE IF NOT EXISTS skills (
            id INTEGER PRIMARY KEY,
            user_id INTEGER NOT NULL,
            skill_id INTEGER NOT NULL,
            skill_name TEXT,
            level INTEGER,
            FOREIGN KEY(user_id) REFERENCES users(id)
        )"#
    ).execute(pool).await?;
    // Ships table
    sqlx::query(
        r#"CREATE TABLE IF NOT EXISTS ships (
            id INTEGER PRIMARY KEY,
            user_id INTEGER NOT NULL,
            ship_id INTEGER NOT NULL,
            ship_name TEXT,
            fit_name TEXT,
            fit_json TEXT,
            created_at DATETIME,
            FOREIGN KEY(user_id) REFERENCES users(id)
        )"#
    ).execute(pool).await?;
    // Modules table
    sqlx::query(
        r#"CREATE TABLE IF NOT EXISTS modules (
            id INTEGER PRIMARY KEY,
            ship_id INTEGER NOT NULL,
            module_id INTEGER NOT NULL,
            module_name TEXT,
            slot_type TEXT,
            FOREIGN KEY(ship_id) REFERENCES ships(id)
        )"#
    ).execute(pool).await?;
    Ok(())
}

/// Import static SDE (Static Data Export) data into the local database
///
/// # Arguments
/// * `pool` - The SQLite connection pool
/// * `sde_path` - Path to the SDE data (e.g., JSON or CSV files)
///
/// # Notes
/// - SDE data includes ships, modules, attributes, etc.
/// - This function should parse the SDE files and populate the ships and modules tables
pub async fn import_sde_data(pool: &SqlitePool, sde_path: &str) -> Result<(), String> {
    // TODO: Parse SDE files (JSON/CSV) and insert ship/module data into DB
    // Example: ships.json, modules.json, etc.
    Err("Not implemented".into())
}

/// Fetch required skills for a given ship or module from the SDE
///
/// # Arguments
/// * `type_id` - The type ID of the ship or module
///
/// # Returns
/// - Vec of (skill_id, skill_name, required_level)
///
/// # Notes
/// - This is a stub. Implement actual DB query based on your SDE schema.
pub async fn get_required_skills_for_type(
    _pool: &sqlx::SqlitePool,
    _type_id: i64,
) -> Vec<(i64, String, i32)> {
    // TODO: Query SDE tables (e.g., invtyperequirements, invtypes, etc.)
    // Example return: vec![(333, "Spaceship Command".to_string(), 4)]
    vec![]
}
