<?php
	//Trim a given string by two internal strings with offsets
	function trimString($wholeString, $beginFrom, $endAt, $startOffset=0, $endOffset=0) {
		$startPosition = strpos($wholeString, $beginFrom) + strlen($beginFrom) + $startOffset;
		$endPosition = strpos($wholeString, $endAt, $startPosition) - $endOffset;
		$difference = $endPosition - $startPosition;
		return substr($wholeString, $startPosition, $difference);
	}
	
	$dl_url = "https://www.mp3sound.cloud/tracks.php?u=".urlencode($_GET['url'])."&t=".urlencode($_GET['url']);
	$dl_html = file_get_contents($dl_url);
	$dl_html_orig = $dl_html;
	
	$art_url = array(); 
	$file_url = array(); 
	$meta_title = array(); 
	$meta_artist = array(); 
	
	//Loop through all entries in playlist (or single track) and grab data
	while (true) {
		$trim_pos = strpos($dl_html, "row bg-white py-3 border-bottom");
		if ($trim_pos == 0) break;
		$dl_html = substr($dl_html, $trim_pos + 20);
		
		$validation = trimString($dl_html, '<a id="mp3Down_', '"');
		
		$this_art_url = trimString($dl_html, "img-fluid rounded card-img-left", '"', 7);
		$this_id = trimString($dl_html, "data-id=", "data-s", 1, 2);
		$this_s = trimString($dl_html, "data-s=", "data-h", 1, 2);
		$this_h = trimString($dl_html, "data-h=", "data-t", 1, 2);
		$this_t = trimString($dl_html, "data-t=", "href", 1, 2);
		
		$this_meta = trimString($dl_html, '<div class="meta">', "</div>");
		$this_title = trimString($this_meta, 'title="Download ', '">');
		$this_artist = trimString($this_meta, "<small>", "<br />");
		
		if ($this_id != $validation) break;
		
		array_push($art_url, $this_art_url);
		array_push($meta_title, $this_title);
		array_push($meta_artist, $this_artist);
		
		//The URL needs to be pulled from a fake origin API
		$this_dl_curl = curl_init("https://www.genmp3.net/dl.v3.php?id=".$this_id."&t=".urlencode($this_t)."&h=".$this_h."&s=".$this_s."&st=");
		curl_setopt($this_dl_curl, CURLOPT_RETURNTRANSFER, TRUE);
		curl_setopt($this_dl_curl, CURLOPT_REFERER, $dl_url);
		$headers = [
			'sec-fetch-dest: empty',
			'sec-fetch-mode: cors',
			'sec-fetch-site: same-origin',
			'x-requested-with: XMLHttpRequest'
		];
		curl_setopt($this_dl_curl, CURLOPT_HTTPHEADER, $headers);
		$file_url_json = curl_exec($this_dl_curl);
		curl_close($this_dl_curl);
		
		array_push($file_url, json_decode($file_url_json)->{'link'});
	}
	
	//Format nicely and download
	if (count($file_url) == 0) {
		$meta_final->error = "Nothing found at provided URL!";
	}
	else {
		$meta_final->error = "Success";
		for ($i = 0; $i < count($file_url); $i++) {
			$meta_final->songs[$i]->metadata->title = $meta_title[$i];
			$meta_final->songs[$i]->metadata->artist = $meta_artist[$i];
			$meta_final->songs[$i]->urls->artwork = $art_url[$i];
			$meta_final->songs[$i]->urls->mp3 = $file_url[$i];
		}
		$meta_final->is_playlist = (count($file_url) != 1);
		if (count($file_url) == 1) $meta_final->name = $meta_title[0];
		else $meta_final->name = trimString($dl_html_orig, "lead mb-0", "</p>", 2);
	}
	header('Content-Type: application/json');
	echo json_encode($meta_final);
?>