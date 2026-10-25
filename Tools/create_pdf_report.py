#!/usr/bin/env python3
"""Create the Persian PDF directly with Chromium's RTL text and font shaping."""
import argparse, html, shutil, subprocess, tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('--repository-url', default=(ROOT/'repository.txt').read_text().strip())
a = p.parse_args()
url = html.escape(a.repository_url, quote=True)
def latin(text): return '<span class="latin" dir="ltr">'+html.escape(text)+'</span>'
def figure(name, caption):
    uri = (ROOT/'Documentation/Screenshots'/name).as_uri()
    return '<figure><img src="'+uri+'" alt="'+html.escape(caption)+'"><figcaption>'+caption+'</figcaption></figure>'
def page(content, number):
    return '<article><main>'+content+'</main><footer>امیرحسین قدردان · شمارهٔ دانشجویی ۴۰۱۵۲۱۴۸۹ · صفحهٔ '+str(number)+'</footer></article>'
windows = (ROOT/'Builds/Windows/DuneDash.exe').is_file()
portable = windows and (ROOT/'Delivery/DuneDash-Portable.exe').is_file()
platform = 'ویندوز ۶۴ بیتی' if windows else 'لینوکس ۶۴ بیتی'
launch = 'DuneDash-Portable.exe' if portable else 'DuneDash.exe' if windows else 'DuneDash.x86_64'
if portable:
    delivery_instructions = 'فقط فایل '+latin(launch)+' را اجرا کنید. تمام فایل‌های لازم داخل این فایل بسته‌بندی شده‌اند و هنگام اجرا خودکار در پوشهٔ موقت باز می‌شوند؛ نگه‌داری DLL یا پوشهٔ داده در کنار آن لازم نیست.'
else:
    delivery_instructions = 'فایل '+latin(launch)+' را همراه با پوشهٔ داده‌ها و کتابخانه‌های کناری آن نگه دارید و اجرا کنید.'
font = Path('/usr/share/fonts/truetype/vazirmatn/Vazirmatn-Regular.ttf')
if not font.exists():
    font = Path(subprocess.check_output(['fc-match','-f','%{file}','sans:lang=fa'],text=True).strip())
summary = '''<header><div class="eyebrow">طراحی بازی‌های کامپیوتری · تمرین اول</div><h1 dir="ltr">DUNE DASH</h1><h2>گزارش بازی دایناسور</h2></header>
<div class="identity"><b>امیرحسین قدردان</b><span>شمارهٔ دانشجویی: ۴۰۱۵۲۱۴۸۹</span></div>
<div class="repo"><b>مخزن پروژه</b><a dir="ltr" href="'''+url+'">'+url+'''</a></div>
<h3>بازی</h3>
<p>دایناسور به‌صورت پیوسته می‌دود و باید از موانع زمینی و هوایی عبور کند. بازی با سه جان شروع می‌شود؛ هر برخورد یک جان کم می‌کند و با تمام شدن جان‌ها، صفحهٔ پایان نمایش داده می‌شود. جمع کردن هر سکه ۲۵ امتیاز دارد و سرعت بازی هم به‌مرور بیشتر می‌شود.</p>
<p>محیط بازی یک ساحل نمکی با صخره‌های آبی است. دایناسور سبزآبی، شال زرد و کوله دارد. ابرها، صخره‌ها و زمین با سرعت‌های متفاوت حرکت می‌کنند تا پس‌زمینه عمق داشته باشد. تصاویر پیکسلی و صداها با اسکریپت‌های همین پروژه تولید شده‌اند.</p>
<h3>کنترل‌ها و منوها</h3>
<p>با '''+latin('Space')+' می‌توان پرید و با '+latin('Shift')+' حرکت سریع انجام داد. کلید '+latin('Esc')+''' بازی را متوقف می‌کند. در منوی توقف، ادامهٔ بازی، شروع دوباره و برگشت به منوی اصلی در دسترس است. جان‌ها، امتیاز و رکورد بالای صفحه دیده می‌شوند. رکورد با '''+latin('PlayerPrefs')+''' ذخیره می‌شود و بعد از بستن بازی هم باقی می‌ماند.</p>
<h3>ساختار کد</h3>
<p>پروژه با '''+latin('Unity 2022.3.62f1')+' و زبان '+latin('C#')+' ساخته شده است. ورودی‌ها از '+latin('New Input System 1.7.0')+' خوانده می‌شوند. '+latin('GameManager')+' امتیاز و وضعیت بازی را نگه می‌دارد و '+latin('DinoController')+' مسئول حرکت و جان‌های دایناسور است. '+latin('ObstacleSpawner')+''' موانع و سکه‌ها را تولید می‌کند.</p>
<p>کلاس‌های '''+latin('GroundObstacle')+'، '+latin('FlyingObstacle')+' و '+latin('Coin')+' از '+latin('Interactable')+' ارث می‌برند. حرکت مشترک آن‌ها در کلاس پایه و رفتار برخورد در کلاس‌های فرزند قرار دارد. انیمیشن دایناسور با '+latin('Animator')+' و چهار حالت '+latin('Run / Jump / Hit / Death')+''' کنترل می‌شود. هنگام آسیب، رنگ کاراکتر تغییر می‌کند و برای مدت کوتاهی چشمک می‌زند.</p>
<h3>اجرا و بررسی</h3>
<p>نسخهٔ تحویل برای '''+platform+' است. '+delivery_instructions+'''</p>
<p>نسخهٔ لینوکس اجرا شده و هر ۲۵ بررسی خودکار آن موفق بوده است؛ از جمله پرش، حرکت سریع، برخورد، سکه، توقف، ذخیرهٔ رکورد و شروع دوباره. خروجی ویندوز ساخته شده و سلامت بستهٔ اجرایی بررسی شده است، اما اجرای آن روی ویندوز در این سیستم آزمایش نشده است. تصاویر صفحات بعد از اجرای نسخهٔ لینوکس گرفته شده‌اند.</p>
<p class="sources">منابع: صورت تمرین و مستندات رسمی یونیتی. فونت بازی '''+latin('Lato')+' با مجوز '+latin('SIL OFL 1.1')+' است. بستهٔ اجرایی با '+latin('NSIS')+' ساخته شده است. در تهیهٔ کد و مستندات از '+latin('OpenAI Codex')+''' کمک گرفته شده است. لینک منابع و مجوز فونت در پوشهٔ مستندات قرار دارند.</p>'''
images1 = '<h2>منوی اصلی و محیط بازی</h2>'
images1 += figure('01-main-menu.png','تصویر ۱ — منوی اصلی، شروع بازی و راهنمای کنترل‌ها')
images1 += figure('07-gameplay.png','تصویر ۲ — محیط بازی، جان‌ها، امتیاز، موانع و سکه‌ها')
images2 = '<h2>توقف و پایان بازی</h2>'
images2 += figure('04-pause.png','تصویر ۳ — منوی توقف؛ ادامه، شروع دوباره و بازگشت به منو')
images2 += figure('06-game-over.png','تصویر ۴ — پایان بازی، امتیاز نهایی و رکورد ذخیره‌شده')
css = '''@font-face{font-family:Report;src:url("FONT_URI")}@page{size:A4;margin:12mm 14mm}
*{box-sizing:border-box}html,body{margin:0;padding:0}body{font-family:Report,sans-serif;color:#1c3049;font-size:10.8pt;line-height:1.65;direction:rtl}
article{height:271mm;display:flex;flex-direction:column;break-after:page}article:last-child{break-after:auto}main{flex:1}header{text-align:center;border-bottom:2px solid #e4b970;padding-bottom:3mm;margin-bottom:3mm}.eyebrow{font-size:9pt;color:#64787a}h1{font-size:25pt;letter-spacing:2px;line-height:1.15;margin:2mm 0}h2{font-size:14pt;margin:2mm 0 4mm}header h2{font-size:12pt;margin:1mm 0}h3{font-size:11pt;margin:3mm 0 1mm}p{margin:1mm 0 2mm;text-align:justify}.identity{display:flex;justify-content:space-between;font-size:10.5pt;margin:3mm 0}.repo{background:#edf4f3;padding:2mm 3mm;margin:2mm 0}.repo b{display:block;font-size:9.5pt}.repo a{display:block;text-align:left;direction:ltr;unicode-bidi:isolate;font:8.7pt sans-serif;line-height:1.7;color:#1c3049;overflow-wrap:anywhere}.latin{direction:ltr;unicode-bidi:isolate;display:inline-block;font-family:sans-serif;font-size:9pt;vertical-align:baseline}table{border-collapse:collapse;width:100%;font-size:9.1pt;line-height:1.45;margin-top:2mm}th,td{border:1px solid #ced7d3;padding:1.4mm 2mm;text-align:right;vertical-align:top}th{background:#edf1eb}td:first-child{width:24%;font-weight:bold}table .latin{font-size:8.2pt}.sources{font-size:8.2pt;color:#51666a}.sources .latin{font-size:7.6pt}footer{border-top:1px solid #d8dfda;text-align:center;font-size:8pt;color:#64787a;padding-top:2mm;margin-top:2mm}figure{margin:4mm 0 7mm;break-inside:avoid}figure img{width:100%;height:auto;border:1px solid #d5d8c9}figcaption{text-align:center;font-size:10pt;margin-top:2mm}.note{font-size:9pt;color:#64787a}'''.replace('FONT_URI',font.as_uri())
text='<!doctype html><html lang="fa" dir="rtl"><meta charset="utf-8"><title>گزارش بازی دایناسور — امیرحسین قدردان</title><style>'+css+'</style><body>'+page(summary,1)+page(images1,2)+page(images2,3)+'</body></html>'
out = ROOT/'Documentation/GD_Hw1_Amirhossein_Ghadrdan_401521489.pdf'
chrome=shutil.which('google-chrome') or shutil.which('chromium')
if not chrome: raise SystemExit('Chromium or Google Chrome is required to render the PDF.')
with tempfile.TemporaryDirectory(prefix='hm1-pdf-') as tmp:
    tmp=Path(tmp); source=tmp/'report.html';source.write_text(text)
    result=subprocess.run([chrome,'--headless=new','--no-sandbox','--disable-gpu','--disable-dev-shm-usage','--allow-file-access-from-files','--no-pdf-header-footer','--user-data-dir='+str(tmp/'profile'),'--print-to-pdf='+str(out),source.as_uri()],stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True,timeout=60)
    if result.returncode or not out.is_file(): raise SystemExit(result.stderr[-2000:])
print(out)
