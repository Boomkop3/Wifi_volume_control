use std::{env};
use isahc::HttpClient;

fn main() {
    let mut args: Vec<String> = env::args().collect();
    // <ICUE>
    args.push(String::from("UP"));
    // args.push(String::from("DOWN"));
    args.push(String::from("192.168.0.150"));
    // </ICUE>
    if  args.len() != 3 {
        print_help();
        return;
    }
    match args[1].as_str() {
        "UP" => handle_volume_change_result(
            change_volume(VolumeDirection::UP, args[2].as_str())
        ),
        "DOWN" => handle_volume_change_result(
            change_volume(VolumeDirection::DOWN, args[2].as_str())
        ),
        _ => print_help()
    }
}

fn handle_volume_change_result(result:Result<(), ()>){
    if result.is_ok(){
        print!("command transmitted successfully");
    } else if result.is_err() {
        print!("ERROR: ");
    }
}

fn change_volume(direction: VolumeDirection, ip: &str) -> Result<(), ()> {
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
    let url:String = format!("http://{}/MainZone/index.put.asp", ip);
    let command:String = format!("cmd0=PutMasterVolumeBtn/{}", volume_char);
    print!("url: {}\r\n", url);
    print!("command: {}\r\n", command);
    print!("transmitting signal now\r\n");
    let client = client.post(
        url,
        command
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