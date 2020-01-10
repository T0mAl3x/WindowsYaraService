@echo off
SET path_to_scrapper=../../../YaraRulesScrapper
CD %path_to_scrapper%

SET download_path=%1
.\venv\Scripts\activate & scrapy crawl yara_spider -a files_path=download_path