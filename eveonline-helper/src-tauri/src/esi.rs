//! ESI (EVE SSO) OAuth2 PKCE authentication logic for EveOnline Helper
//
// This module will handle the OAuth2 PKCE flow for secure EVE SSO login.
// It will expose functions to start the login process, handle callbacks, and manage tokens securely.
//
// Dependencies: oauth2, url, reqwest, tauri (for command exposure), keyring (for secure storage)

use keyring::Entry;
use reqwest::Client;
use serde::Deserialize;
use std::collections::HashMap;
use std::fs;
use std::path::Path;
use std::time::{SystemTime, UNIX_EPOCH};
use tauri::api::shell;
use tauri::Window;

/// Configuration for EVE SSO OAuth2 endpoints and client credentials
pub struct EveSsoConfig {
    /// EVE SSO client ID (register at https://developers.eveonline.com/)
    pub client_id: String,
    /// Redirect URI (must match registered app)
    pub redirect_uri: String,
    /// EVE SSO authorization endpoint
    pub auth_url: String,
    /// EVE SSO token endpoint
    pub token_url: String,
    /// ESI scopes required for the app
    pub scopes: Vec<String>,
}

/// PKCE challenge and verifier pair
pub struct PkcePair {
    pub challenge: String,
    pub verifier: String,
}

/// Represents an OAuth2 token response
pub struct TokenResponse {
    pub access_token: String,
    pub refresh_token: Option<String>,
    pub expires_in: u64,
    pub token_type: String,
}

/// ESI character info response
#[derive(Debug, Deserialize)]
pub struct CharacterInfo {
    pub character_id: i64,
    pub name: String,
    // Add more fields as needed
}

/// ESI skills response
#[derive(Debug, Deserialize)]
pub struct Skill {
    pub skill_id: i64,
    pub skill_name: Option<String>,
    pub active_level: i32,
    // Add more fields as needed
}

/// Ship and Module structs for SDE data (to be expanded)
#[derive(Debug, Clone)]
pub struct Ship {
    pub ship_id: i64,
    pub ship_name: String,
    // Add more fields as needed (attributes, required skills, etc.)
}

#[derive(Debug, Clone)]
pub struct Module {
    pub module_id: i64,
    pub module_name: String,
    // Add more fields as needed (slot type, required skills, etc.)
}

/// Fit variant struct for ship fitting recommendations
#[derive(Debug, Clone)]
pub struct FitVariant {
    pub fit_name: String,
    pub ship: Ship,
    pub modules: Vec<Module>,
    pub rationale: String, // Explanation for the fit
                           // Add more fields as needed (performance metrics, etc.)
}

/// Validation result for a fit
#[derive(Debug, Clone)]
pub struct FitValidation {
    pub is_valid: bool,
    pub missing_requirements: Vec<String>,
    pub warnings: Vec<String>,
}

/// Enum representing basic ship tiers (expand as needed)
#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord)]
pub enum ShipTier {
    Frigate,
    Destroyer,
    Cruiser,
    Battlecruiser,
    Battleship,
    // Add more as needed
}

/// Validate a fit for powergrid, CPU, slots, and skill requirements
///
/// # Arguments
/// * `fit` - The fit variant to validate
/// * `user_skills` - The user's skills
///
/// # Returns
/// - FitValidation struct with validation results
pub fn validate_fit(fit: &FitVariant, user_skills: &[Skill]) -> FitValidation {
    // TODO: Check powergrid, CPU, slot layout, and required skills
    // Populate missing_requirements and warnings as needed
    FitValidation {
        is_valid: false,
        missing_requirements: vec!["Not implemented".into()],
        warnings: vec![],
    }
}

/// Starts the OAuth2 PKCE login flow
pub fn start_login(config: &EveSsoConfig) -> Result<(String, PkcePair), String> {
    // TODO: Generate PKCE challenge/verifier, build auth URL, return to frontend
    Err("Not implemented".into())
}

/// Handles the OAuth2 callback and exchanges code for tokens
pub fn handle_callback(
    config: &EveSsoConfig,
    pkce: &PkcePair,
    code: &str,
) -> Result<TokenResponse, String> {
    // TODO: Exchange code for tokens using PKCE verifier
    Err("Not implemented".into())
}

/// Refreshes the access token using the refresh token
pub fn refresh_token(config: &EveSsoConfig, refresh_token: &str) -> Result<TokenResponse, String> {
    // TODO: Implement token refresh logic
    Err("Not implemented".into())
}

/// Opens the system browser to the EVE SSO login URL
pub fn open_browser_for_login(window: &Window, auth_url: &str) -> Result<(), String> {
    // Use Tauri's shell API to open the system browser
    shell::open(&window.shell_scope(), auth_url.to_string(), None)
        .map_err(|e| format!("Failed to open browser: {}", e))
}

/// Handles the local callback after EVE SSO login
pub fn handle_local_callback(_window: &Window, _callback_url: &str) -> Result<String, String> {
    // TODO: Parse the callback URL, extract the authorization code, and continue the OAuth2 flow
    Err("Not implemented".into())
}

/// Stores the ESI access and refresh tokens securely in the OS keychain
pub fn store_tokens(
    user_id: &str,
    access_token: &str,
    refresh_token: Option<&str>,
) -> Result<(), String> {
    let service = "eveonline-helper-esi";
    let access_entry = Entry::new(service, &format!("{}_access", user_id))
        .map_err(|e| format!("Keyring error: {}", e))?;
    access_entry
        .set_password(access_token)
        .map_err(|e| format!("Failed to store access token: {}", e))?;
    if let Some(refresh) = refresh_token {
        let refresh_entry = Entry::new(service, &format!("{}_refresh", user_id))
            .map_err(|e| format!("Keyring error: {}", e))?;
        refresh_entry
            .set_password(refresh)
            .map_err(|e| format!("Failed to store refresh token: {}", e))?;
    }
    Ok(())
}

/// Retrieves the ESI access and refresh tokens from the OS keychain
pub fn get_tokens(user_id: &str) -> Result<(String, Option<String>), String> {
    let service = "eveonline-helper-esi";
    let access_entry = Entry::new(service, &format!("{}_access", user_id))
        .map_err(|e| format!("Keyring error: {}", e))?;
    let access_token = access_entry
        .get_password()
        .map_err(|e| format!("Failed to retrieve access token: {}", e))?;
    let refresh_entry = Entry::new(service, &format!("{}_refresh", user_id))
        .map_err(|e| format!("Keyring error: {}", e))?;
    let refresh_token = match refresh_entry.get_password() {
        Ok(token) => Some(token),
        Err(_) => None,
    };
    Ok((access_token, refresh_token))
}

/// Fetch character info from ESI
pub async fn fetch_character_info(
    access_token: &str,
    character_id: i64,
) -> Result<CharacterInfo, String> {
    let url = format!(
        "https://esi.evetech.net/latest/characters/{}/?datasource=tranquility",
        character_id
    );
    let client = Client::new();
    let resp = client
        .get(&url)
        .bearer_auth(access_token)
        .send()
        .await
        .map_err(|e| format!("Failed to fetch character info: {}", e))?;
    resp.json::<CharacterInfo>()
        .await
        .map_err(|e| format!("Failed to parse character info: {}", e))
}

/// Fetch character skills from ESI
pub async fn fetch_character_skills(
    access_token: &str,
    character_id: i64,
) -> Result<Vec<Skill>, String> {
    let url = format!(
        "https://esi.evetech.net/latest/characters/{}/skills/?datasource=tranquility",
        character_id
    );
    let client = Client::new();
    let resp = client
        .get(&url)
        .bearer_auth(access_token)
        .send()
        .await
        .map_err(|e| format!("Failed to fetch skills: {}", e))?;
    // The ESI response wraps skills in a "skills" array
    #[derive(Deserialize)]
    struct SkillsWrapper {
        skills: Vec<Skill>,
    }
    let wrapper = resp
        .json::<SkillsWrapper>()
        .await
        .map_err(|e| format!("Failed to parse skills: {}", e))?;
    Ok(wrapper.skills)
}

/// Match user skills to eligible ships and modules
///
/// # Arguments
/// * `user_skills` - List of user skills (Skill structs)
/// * `all_ships` - List of all ships from SDE
/// * `all_modules` - List of all modules from SDE
///
/// # Returns
/// - List of ships and modules the user is eligible to use
pub fn match_skills_to_ships_and_modules(
    user_skills: &[Skill],
    all_ships: &[Ship],
    all_modules: &[Module],
) -> (Vec<Ship>, Vec<Module>) {
    // TODO: Filter ships and modules based on user skill levels
    // Example: Only include ships/modules where user meets all required skills
    (vec![], vec![])
}

/// Generate five fit variants per ship/activity
///
/// # Arguments
/// * `ship` - The ship to generate fits for
/// * `activity` - The selected activity (e.g., "Mission Running", "PVP")
/// * `eligible_modules` - List of modules the user can use
///
/// # Returns
/// - List of five recommended fit variants
pub fn generate_fit_variants(
    ship: &Ship,
    activity: &str,
    eligible_modules: &[Module],
) -> Vec<FitVariant> {
    // TODO: Implement logic for:
    // - Maximum DPS
    // - Maximum Tank
    // - Balanced
    // - Cap-Stable
    // - Activity-Optimized
    vec![]
}

/// Suggest alternative fits if requirements are not met
///
/// # Arguments
/// * `fit` - The original fit variant
/// * `user_skills` - The user's skills
/// * `eligible_modules` - List of modules the user can use
///
/// # Returns
/// - List of alternative FitVariant suggestions
pub fn suggest_alternative_fits(
    fit: &FitVariant,
    user_skills: &[Skill],
    eligible_modules: &[Module],
) -> Vec<FitVariant> {
    // TODO: Generate alternative fits by replacing modules or downgrading requirements
    vec![]
}

/// Generate a prioritized skill plan for a recommended fit
///
/// # Arguments
/// * `fit` - The recommended fit variant (ship + modules)
/// * `user_skills` - The user's current skills
///
/// # Returns
/// - Ordered list of (skill_id, skill_name, required_level, current_level) for missing or under-leveled skills
///
/// # Logic
/// - For each module and the ship in the fit, collect all required skills and levels
/// - Compare with user_skills; if user is missing a skill or has insufficient level, add to plan
/// - Order the plan by fit unlock dependencies (e.g., ship skills first, then modules)
/// - No duplicate skills; if multiple modules require the same skill at different levels, use the highest required
pub fn generate_skill_plan_for_fit(
    fit: &FitVariant,
    user_skills: &[Skill],
    // pool: Option<&sqlx::SqlitePool>, // Uncomment if you want to pass DB pool for real SDE lookup
) -> Vec<(i64, String, i32, i32)> {
    use std::collections::HashMap;
    // Map skill_id -> (skill_name, required_level)
    let mut required_skills: HashMap<i64, (String, i32)> = HashMap::new();
    // Helper: add or update required skill if higher level needed
    let mut add_required = |skill_id: i64, skill_name: &str, level: i32| {
        required_skills
            .entry(skill_id)
            .and_modify(|e| {
                if level > e.1 {
                    e.1 = level;
                }
            })
            .or_insert((skill_name.to_string(), level));
    };
    // --- SDE DB lookup (async) would go here ---
    // Example:
    // if let Some(pool) = pool {
    //     let ship_skills = db::get_required_skills_for_type(pool, fit.ship.ship_id).await;
    //     for (skill_id, skill_name, level) in ship_skills { add_required(skill_id, &skill_name, level); }
    //     for module in &fit.modules {
    //         let module_skills = db::get_required_skills_for_type(pool, module.module_id).await;
    //         for (skill_id, skill_name, level) in module_skills { add_required(skill_id, &skill_name, level); }
    //     }
    // } else {
    //     // Fallback placeholder logic
    // }
    // --- End SDE DB lookup ---
    // Placeholder: assume ship requires "Spaceship Command" level 4
    add_required(333, "Spaceship Command", 4);
    // Placeholder: assume all modules require "Engineering" level 3
    for module in &fit.modules {
        add_required(444, "Engineering", 3);
    }
    // Map user skills for quick lookup
    let mut user_skill_map: HashMap<i64, (String, i32)> = HashMap::new();
    for s in user_skills {
        user_skill_map.insert(
            s.skill_id,
            s.skill_name.clone().unwrap_or_default(),
            s.active_level,
        );
    }
    // Build the plan: (skill_id, skill_name, required_level, current_level)
    let mut plan = vec![];
    for (skill_id, (skill_name, required_level)) in required_skills.iter() {
        let current_level = user_skill_map
            .get(skill_id)
            .map(|(_, lvl)| *lvl)
            .unwrap_or(0);
        if current_level < *required_level {
            plan.push((
                *skill_id,
                skill_name.clone(),
                *required_level,
                current_level,
            ));
        }
    }
    // Order: ship skills first (id 333), then others
    plan.sort_by_key(|(skill_id, _, _, _)| if *skill_id == 333 { 0 } else { 1 });
    plan
}

/// Suggest the next ship tier and a target ship for user progression
///
/// # Arguments
/// * `user_skills` - The user's current skills
/// * `eligible_ships` - Ships the user can currently fly
/// * `all_ships` - All ships in the SDE
///
/// # Returns
/// - (next_tier, suggested_ship, required_skills)
///
/// # Notes
/// - This is a stub. Real logic should use SDE data to determine ship tiers and requirements.
pub fn suggest_next_ship_tier(
    user_skills: &[Skill],
    eligible_ships: &[Ship],
    all_ships: &[Ship],
) -> Option<(ShipTier, Ship, Vec<(i64, String, i32)>)> {
    // Placeholder: Assume tiers are ordered as in ShipTier enum, and ship_name contains tier
    let current_tier = eligible_ships
        .iter()
        .filter_map(|ship| {
            if ship.ship_name.contains("Frigate") {
                Some(ShipTier::Frigate)
            } else if ship.ship_name.contains("Destroyer") {
                Some(ShipTier::Destroyer)
            } else if ship.ship_name.contains("Cruiser") {
                Some(ShipTier::Cruiser)
            } else if ship.ship_name.contains("Battlecruiser") {
                Some(ShipTier::Battlecruiser)
            } else if ship.ship_name.contains("Battleship") {
                Some(ShipTier::Battleship)
            } else {
                None
            }
        })
        .max();
    let next_tier = match current_tier {
        Some(ShipTier::Frigate) => ShipTier::Destroyer,
        Some(ShipTier::Destroyer) => ShipTier::Cruiser,
        Some(ShipTier::Cruiser) => ShipTier::Battlecruiser,
        Some(ShipTier::Battlecruiser) => ShipTier::Battleship,
        _ => ShipTier::Frigate,
    };
    // Find a ship in the next tier
    let suggested_ship = all_ships.iter().find(|ship| match next_tier {
        ShipTier::Destroyer => ship.ship_name.contains("Destroyer"),
        ShipTier::Cruiser => ship.ship_name.contains("Cruiser"),
        ShipTier::Battlecruiser => ship.ship_name.contains("Battlecruiser"),
        ShipTier::Battleship => ship.ship_name.contains("Battleship"),
        ShipTier::Frigate => ship.ship_name.contains("Frigate"),
    });
    if let Some(ship) = suggested_ship {
        // TODO: Lookup required skills for this ship from SDE
        let required_skills = vec![(333, "Spaceship Command".to_string(), 5)];
        Some((next_tier, ship.clone(), required_skills))
    } else {
        None
    }
}

/// Export a skill plan as an EVEMon-compatible string
///
/// # Arguments
/// * `plan` - List of (skill_id, skill_name, required_level, current_level)
///
/// # Returns
/// - String in EVEMon format: '[Skill Name] Level X' per line
pub fn export_skill_plan_evemon(plan: &[(i64, String, i32, i32)]) -> String {
    plan.iter()
        .map(|(_, skill_name, required_level, _)| {
            format!("{} Level {}", skill_name, required_level)
        })
        .collect::<Vec<_>>()
        .join("\n")
}

/// Download and update the SDE snapshot.
/// Returns Ok(()) on success, Err(msg) on failure.
pub fn refresh_sde() -> Result<(), String> {
    // Example SDE URL (replace with actual, e.g., YAML or SQLite snapshot)
    let sde_url = "https://eve-static-data-export.s3-eu-west-1.amazonaws.com/tranquility/sde.zip";
    let sde_path = "data/sde.zip";
    // Download SDE
    let resp =
        reqwest::blocking::get(sde_url).map_err(|e| format!("Failed to download SDE: {}", e))?;
    let bytes = resp
        .bytes()
        .map_err(|e| format!("Failed to read SDE bytes: {}", e))?;
    fs::create_dir_all("data").map_err(|e| format!("Failed to create data dir: {}", e))?;
    fs::write(sde_path, &bytes).map_err(|e| format!("Failed to save SDE: {}", e))?;
    // TODO: Unzip and update DB from SDE
    // Update last update timestamp
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap()
        .as_secs();
    fs::write("data/last_sde_update.txt", now.to_string())
        .map_err(|e| format!("Failed to write timestamp: {}", e))?;
    Ok(())
}

/// Check if SDE update is needed (returns true if >24h since last update)
pub fn sde_update_needed() -> bool {
    let path = Path::new("data/last_sde_update.txt");
    if let Ok(contents) = fs::read_to_string(path) {
        if let Ok(last) = contents.parse::<u64>() {
            let now = SystemTime::now()
                .duration_since(UNIX_EPOCH)
                .unwrap()
                .as_secs();
            return now > last + 24 * 3600;
        }
    }
    true // If missing or parse error, force update
}

/// Check if EULA has been accepted (returns true if accepted)
pub fn eula_accepted() -> bool {
    fs::read_to_string("data/eula_accepted.txt")
        .unwrap_or_default()
        .trim()
        == "yes"
}

/// Set EULA acceptance (true/false)
pub fn set_eula_accepted(accepted: bool) -> Result<(), String> {
    fs::create_dir_all("data").map_err(|e| format!("Failed to create data dir: {}", e))?;
    fs::write(
        "data/eula_accepted.txt",
        if accepted { "yes" } else { "no" },
    )
    .map_err(|e| format!("Failed to write EULA status: {}", e))
}

// TODO: Add logic to update the local database with fetched character and skill data
