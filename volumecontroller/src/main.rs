use std::{env};
use isahc::HttpClient;

fn main() {
    let args: Vec<String> = env::args().collect();
    if  args.len() != 2 {
        print_help();
        return;
    }
    match args[1].as_str() {
        "UP" => change_volume(VolumeDirection::UP).unwrap(), 
        "DOWN" => change_volume(VolumeDirection::DOWN).unwrap(), 
        _ => print_help()
    }
}

fn change_volume(direction: VolumeDirection) -> Result<(), ()> {
    let client = HttpClient::new();
    if client.is_err(){
        return Err(());
    }
    let client = client.unwrap();
    let volume_char;
    match direction {
        VolumeDirection::UP => volume_char = ">",
        VolumeDirection::DOWN => volume_char = "<"
    }
    let client = client.post(
        "http://192.168.0.150/MainZone/index.put.asp", 
        format!("cmd0=PutMasterVolumeBtn/{}", volume_char)
    );
    if client.is_err() { return Err(()); }
    return Ok(());
}

enum VolumeDirection {
    UP, DOWN
}

fn print_help(){
    println!("");
    println!("Simple Volume Command Sending Thingy");
    println!("Compatible with DENON AVR 1912");
    println!("");
    println!("usage: \r\n\tvolumecontroller.exe UP/DOWN [hostname]");
    println!("example: \r\n\tvolumecontroller.exe UP 192.168.0.100");
}