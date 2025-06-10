// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
#[tauri::command]
fn greet(name: &str) -> String {
    format!("Hello, {}! You've been greeted from Rust!", name)
}

mod esi;
use esi::{EveSsoConfig, open_browser_for_login, handle_local_callback, match_skills_to_ships_and_modules, generate_fit_variants, validate_fit, suggest_alternative_fits, Ship, Module, FitVariant, Skill};
use tauri::Window;

/// Tauri command to start the EVE SSO login flow and open the system browser
#[tauri::command]
pub fn start_eve_sso_login(window: Window, auth_url: String) -> Result<(), String> {
    open_browser_for_login(&window, &auth_url)
}

/// Tauri command to handle the local callback after EVE SSO login
#[tauri::command]
pub fn process_eve_sso_callback(window: Window, callback_url: String) -> Result<String, String> {
    handle_local_callback(&window, &callback_url)
}

/// Tauri command to get ship fit recommendations for a user
#[tauri::command]
pub async fn get_fit_recommendations(
    user_skills: Vec<Skill>,
    all_ships: Vec<Ship>,
    all_modules: Vec<Module>,
    activity: String,
) -> Vec<FitVariant> {
    // 1. Match user skills to eligible ships and modules
    let (eligible_ships, eligible_modules) = match_skills_to_ships_and_modules(&user_skills, &all_ships, &all_modules);
    // 2. For each eligible ship, generate fit variants
    let mut recommendations = vec![];
    for ship in eligible_ships {
        let fits = generate_fit_variants(&ship, &activity, &eligible_modules);
        // 3. Validate each fit and suggest alternatives if needed
        for fit in fits {
            let validation = validate_fit(&fit, &user_skills);
            if validation.is_valid {
                recommendations.push(fit);
            } else {
                let alternatives = suggest_alternative_fits(&fit, &user_skills, &eligible_modules);
                recommendations.extend(alternatives);
            }
        }
    }
    recommendations
}

/// Tauri command to generate a prioritized skill plan for a recommended fit
///
/// # Arguments
/// * `fit` - The recommended fit variant (from frontend)
/// * `user_skills` - The user's current skills (from frontend)
///
/// # Returns
/// - Ordered list of (skill_id, skill_name, required_level, current_level) for missing or under-leveled skills
#[tauri::command]
pub fn get_skill_plan_for_fit(
    fit: FitVariant,
    user_skills: Vec<Skill>,
) -> Vec<(i64, String, i32, i32)> {
    println!("[SkillPlan] Received request for fit: {}", fit.fit_name);
    let plan = esi::generate_skill_plan_for_fit(&fit, &user_skills);
    println!("[SkillPlan] Generated plan with {} missing/under-leveled skills", plan.len());
    for (skill_id, skill_name, required, current) in &plan {
        println!("[SkillPlan] Skill: {} (ID: {}), Required: {}, Current: {}", skill_name, skill_id, required, current);
    }
    plan
}

/// Tauri command to suggest the next ship progression for the user
///
/// # Arguments
/// * `user_skills` - The user's current skills
/// * `eligible_ships` - Ships the user can currently fly
/// * `all_ships` - All ships in the SDE
///
/// # Returns
/// - (tier, ship, required_skills) or null
#[tauri::command]
pub fn get_next_ship_progression(
    user_skills: Vec<Skill>,
    eligible_ships: Vec<Ship>,
    all_ships: Vec<Ship>,
) -> Option<(String, Ship, Vec<(i64, String, i32)>)> {
    let result = esi::suggest_next_ship_tier(&user_skills, &eligible_ships, &all_ships);
    if let Some((tier, ship, required_skills)) = &result {
        println!("[Progression] Next tier: {:?}, Suggested ship: {}", tier, ship.ship_name);
    } else {
        println!("[Progression] No next ship progression found.");
    }
    // Convert tier to string for frontend
    result.map(|(tier, ship, required_skills)| (format!("{:?}", tier), ship, required_skills))
}

/// Tauri command to export a skill plan as an EVEMon-compatible string
///
/// # Arguments
/// * `plan` - List of (skill_id, skill_name, required_level, current_level)
///
/// # Returns
/// - String in EVEMon format
#[tauri::command]
pub fn export_skill_plan_evemon_cmd(
    plan: Vec<(i64, String, i32, i32)>,
) -> String {
    println!("[Export] Exporting skill plan to EVEMon format ({} skills)", plan.len());
    esi::export_skill_plan_evemon(&plan)
}

#[tauri::command]
pub fn refresh_sde_cmd() -> Result<(), String> {
    esi::refresh_sde()
}

static mut SDE_AUTO_UPDATED: bool = false;

#[tauri::command]
pub fn sde_auto_update_occurred() -> bool {
    unsafe { SDE_AUTO_UPDATED }
}

#[tauri::command]
pub fn get_eula_accepted() -> bool {
    esi::eula_accepted()
}

#[tauri::command]
pub fn set_eula_accepted_cmd(accepted: bool) -> Result<(), String> {
    esi::set_eula_accepted(accepted)
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    // On startup, check if SDE update is needed and refresh in background if so
    if esi::sde_update_needed() {
        println!("[SDE] More than 24h since last SDE update, refreshing in background...");
        unsafe { SDE_AUTO_UPDATED = true; }
        std::thread::spawn(|| {
            let _ = esi::refresh_sde();
        });
    }
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![greet, start_eve_sso_login, process_eve_sso_callback, get_fit_recommendations, get_skill_plan_for_fit, get_next_ship_progression, export_skill_plan_evemon_cmd, refresh_sde_cmd, sde_auto_update_occurred, get_eula_accepted, set_eula_accepted_cmd])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
