function scClick(element, evt) {
  evt = scForm.lastEvent ? scForm.lastEvent : evt;
  var icon = evt.srcElement ? evt.srcElement : evt.target;
  
  var edit = scForm.browser.getControl("IconFile");
  
  if (!icon) {
    return;
  }
   
  var src = scForm.browser.isIE ? icon["sc_path"] : icon.getAttribute("sc_path");

  if (src==null && icon.tagName && icon.tagName.toLowerCase() == "img" && icon.className == "scRecentIcon") {
    src = icon.src;
  }
   
  if (src == null) {
    return;
  }
  
  var n = src.indexOf("/-/icon/");
  
  if (n >= 0) {
    src = src.substr(n + 8); 
  }
  else if (src.substr(0, 32) == "/sitecore/shell/themes/standard/") {
    src = src.substr(32);
  }
  
  n = src.toLowerCase().indexOf(scForm.Settings.Icons.CacheFolder.toLowerCase());
  if (n >= 0) {
    src = src.substr(n + scForm.Settings.Icons.CacheFolder.length);
    if (src[0] === '/') {
      src = src.substring(1);
    }
  }
  
  if (src.substr(src.length - 5, 5) == ".aspx") {
    src = src.substr(0, src.length - 5);
  }
  
  edit.value = src;
}