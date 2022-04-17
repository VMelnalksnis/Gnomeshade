use std::default::Default;
use std::env;

use serde_derive::{Deserialize, Serialize};
use serde_xml_rs::{from_str};

fn main() {
    let args: Vec<String> = env::args().collect();
    if args.len() != 2 {
        panic!("Invalid argument count. Specify a single file to process.");
    }

    let processing_file = &args[1];
    let file_content: String = match std::fs::read_to_string(&processing_file) {
        Ok(file_content) => file_content,
        Err(e) => {
            panic!("Failed to read file: '{}' -- {}", &processing_file, e);
        }
    };

    let report: Report = from_str(&file_content).unwrap();

    let warning_type_ids: Vec<&String> = report.issue_types.issue_types
        .iter()
        .filter(|t| t.severity == "WARNING")
        .map(|t| &t.id)
        .collect();

    let error_type_ids: Vec<&String> = report.issue_types.issue_types
        .iter()
        .filter(|t| t.severity == "ERROR")
        .map(|t| &t.id)
        .collect();

    let warnings = report.issues.projects
        .iter()
        .flat_map(|p| &p.issues)
        .filter(|i| warning_type_ids.contains(&&i.type_id));

    for warning in warnings {
		let filename = warning.file.replace("\\", "/");
		println!("::warning file={0},line={1},endLine={1},col=1,endColumn=1,title={2}::{3}", warning.file, warning.line, warning.type_id, filename);
		println!("::warning file={0},line={1},endLine={1},col=1,endColumn=1,title={2}::{3}", filename, warning.line, warning.type_id, warning.message);
    }

    let errors: Vec<&Issue> = report.issues.projects
        .iter()
        .flat_map(|p| &p.issues)
        .filter(|i| error_type_ids.contains(&&i.type_id))
        .collect();

    for error in errors.iter() {
		let filename = error.file.replace("\\", "/");
		println!("::error file={0},line={1},title={2}::{3}", error.file, error.line, error.type_id, filename);
		println!("::error file={0},line={1},title={2}::{3}", filename, error.line, error.type_id, error.message);
    }

    if errors.len() != 0 {
        panic!("Found {0} errors", errors.len());
    }
}

#[derive(Deserialize, Serialize, Debug)]
struct Report {
    #[serde(rename = "Information")]
    pub information: Information,
    #[serde(rename = "IssueTypes")]
    pub issue_types: IssueTypes,
    #[serde(rename = "Issues")]
    pub issues: Issues,
}

#[derive(Deserialize, Serialize, Debug)]
struct Information {
    #[serde(rename = "Solution")]
    pub solution: String,
    #[serde(rename = "InspectionScope")]
    pub inspection_scope: InspectionScope,
}

#[derive(Default, Deserialize, Serialize, Debug)]
struct InspectionScope {
    #[serde(rename = "Element")]
    pub element: String,
}

#[derive(Default, Deserialize, Serialize, Debug)]
struct IssueTypes {
    #[serde(rename = "$value")]
    pub issue_types: Vec<IssueType>,
}

#[derive(Default, Deserialize, Serialize, Debug)]
struct IssueType {
    #[serde(rename = "Id")]
    pub id: String,
    #[serde(rename = "Category")]
    pub category: String,
    #[serde(rename = "CategoryId")]
    pub category_id: String,
    #[serde(rename = "Description")]
    pub description: String,
    #[serde(rename = "Severity")]
    pub severity: String,
}

#[derive(Default, Deserialize, Serialize, Debug)]
struct Issues {
    #[serde(rename = "$value")]
    pub projects: Vec<Project>,
}

#[derive(Default, Deserialize, Serialize, Debug)]
struct Project {
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "$value")]
    pub issues: Vec<Issue>,
}

#[derive(Default, Deserialize, Serialize, Debug)]
struct Issue {
    #[serde(rename = "TypeId")]
    pub type_id: String,
    #[serde(rename = "File")]
    pub file: String,
    #[serde(rename = "Offset")]
    pub offset: String,
    #[serde(rename = "Line")]
    pub line: u32,
    #[serde(rename = "Message")]
    pub message: String,
}
