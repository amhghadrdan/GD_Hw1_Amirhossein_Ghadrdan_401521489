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
platform = 'ویندوز ۶۴ بیتی' if windows else 'لینوکس ۶۴ بیتی'
launch = 'DuneDash.exe' if windows else 'DuneDash.x86_64'
font = Path('/usr/share/fonts/truetype/vazirmatn/Vazirmatn-Regular.ttf')
if not font.exists():
    font = Path(subprocess.check_output(['fc-match','-f','%{file}','sans:lang=fa'],text=True).strip())
rows = [
 ('محیط و گرافیک','ماه حلقه‌دار، ابرهای کشیده، سه لایهٔ صخرهٔ آبی و زمین نمکی با حرکت چندلایه، حس عمق و دویدن ایجاد می‌کنند.'),
 ('ورودی جدید','تمام ورودی‌ها با '+latin('Unity New Input System 1.7.0')+' و رابط با '+latin('InputSystemUIInputModule')+' مدیریت می‌شوند.'),
 ('کنترل و سلامت','بازیکن سه جان دارد. '+latin('Space')+' برای پرش و '+latin('Shift')+' برای حرکت سریع است. هر برخورد یک جان کم می‌کند؛ با صفر شدن جان‌ها بازی پایان می‌یابد.'),
 ('طراحی شی‌گرا','کلاس‌های '+latin('GroundObstacle')+'، '+latin('FlyingObstacle')+' و '+latin('Coin')+' همگی از کلاس پایهٔ '+latin('Interactable')+' ارث می‌برند.'),
 ('انیمیشن','چهار حالت '+latin('Run / Jump / Hit / Death')+' با مؤلفهٔ '+latin('Animator')+' و انتقال‌های واقعی ساخته شده‌اند. آسیب با تغییر رنگ و چشمک‌زدن نمایش داده می‌شود.'),
 ('رابط و رکورد','منوی اصلی و منوی توقف مستقل‌اند. امتیاز جاری و رکورد به‌صورت زنده نمایش داده می‌شوند و رکورد با '+latin('PlayerPrefs')+' ذخیره می‌شود.')]
summary = '''<header><div class="eyebrow">طراحی بازی‌های کامپیوتری · تمرین اول</div><h1 dir="ltr">DUNE DASH</h1><h2>گزارش پیاده‌سازی بازی دایناسور</h2></header>
<div class="identity"><b>امیرحسین قدردان</b><span>شمارهٔ دانشجویی: ۴۰۱۵۲۱۴۸۹</span></div>
<div class="repo"><b>آدرس مخزن کد در گیت‌هاب</b><a dir="ltr" href="'''+url+'">'+url+'''</a></div>
<h3>ایده و ویژگی‌ها</h3><p>این بازی یک رانر دوبعدی بی‌پایان با تم ساحل نمکی است. کاراکتر سبزآبی با شال و کوله از صخره‌های بلوری و پرندگان سفره‌ماهی‌مانند عبور می‌کند و سکه‌های تراش‌خورده جمع می‌کند. هر سکه ۲۵ امتیاز دارد و سرعت بازی به‌تدریج افزایش می‌یابد. تصاویر پیکسلی، موسیقی و افکت‌های صوتی برای همین پروژه تولید شده‌اند.</p>
<table><thead><tr><th>بخش تمرین</th><th>روش پیاده‌سازی</th></tr></thead><tbody>'''
summary += ''.join('<tr><td>'+label+'</td><td>'+description+'</td></tr>' for label,description in rows)
summary += '''</tbody></table><h3>ساختار و بررسی اجرا</h3><p>مدیریت چرخهٔ بازی و امتیاز در '''+latin('GameManager')+'، حرکت و سلامت در '+latin('DinoController')+' و تولید موانع در '+latin('ObstacleSpawner')+''' انجام می‌شود. رابط، صدا و جلوه‌ها در کلاس‌های مستقل نگه‌داری شده‌اند.</p>
<p>نسخهٔ لینوکس روی همین سیستم اجرا و با ۲۵ آزمون خودکار بررسی شده است؛ همهٔ آزمون‌ها موفق بوده‌اند. این بررسی‌ها شامل ورودی، پرش، حرکت سریع، سکه، برخوردها، توقف، پایان، ذخیرهٔ رکورد و شروع مجدد است.</p>
<h3>روش اجرای خروجی تحویل</h3><p>خروجی ارائه‌شده برای '''+platform+' است. فایل '+latin(launch)+''' را همراه با پوشهٔ داده‌ها و کتابخانه‌های کناری آن نگه دارید و اجرا کنید. در یونیتی نیز صحنهٔ '''+latin('Desert.unity')+''' را باز کنید و دکمهٔ اجرا را بزنید.</p>
<p class="sources">منابع: صورت تمرین و مستندات رسمی یونیتی برای ورودی، انیمیشن و ذخیرهٔ رکورد. فونت بازی لاتـو با مجوز آزاد است. کد و مستندات با کمک ابزار '''+latin('OpenAI Codex')+''' تهیه شده‌اند. جزئیات منابع در '''+latin('Documentation/SOURCES.md')+''' موجود است.</p>'''
images1 = '<h2>تصاویر بازی؛ شروع و گیم‌پلی</h2><p class="note">تصاویر از اجرای واقعی همین سورس در لینوکس گرفته شده‌اند.</p>'
images1 += figure('01-main-menu.png','تصویر ۱ — منوی اصلی، شروع بازی و راهنمای کنترل‌ها')
images1 += figure('07-gameplay.png','تصویر ۲ — محیط بازی، جان‌ها، امتیاز، موانع و سکه‌ها')
images2 = '<h2>تصاویر بازی؛ توقف و پایان</h2>'
images2 += figure('04-pause.png','تصویر ۳ — منوی توقف؛ ادامه، شروع دوباره و بازگشت به منو')
images2 += figure('06-game-over.png','تصویر ۴ — پایان بازی، امتیاز نهایی و رکورد ذخیره‌شده')
css = '''@font-face{font-family:Report;src:url("FONT_URI")}@page{size:A4;margin:12mm 14mm}
*{box-sizing:border-box}html,body{margin:0;padding:0}body{font-family:Report,sans-serif;color:#1c3049;font-size:10.2pt;line-height:1.55;direction:rtl}
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
