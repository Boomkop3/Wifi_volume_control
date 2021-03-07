use isahc::HttpClient;

fn main() {
    change_volume(VolumeDirection::UP).unwrap();
}

fn change_volume(_direction: VolumeDirection) -> Result<(), ()> {
    let client = HttpClient::new();
    if client.is_err(){
        return Err(());
    }
    let client = client.unwrap();
    let volume_char = ">";

    

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
