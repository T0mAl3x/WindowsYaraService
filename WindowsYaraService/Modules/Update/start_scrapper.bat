@echo off
SET path_to_scrapper=../../../YaraRulesScrapper
CD %path_to_scrapper%

.\venv\Scripts\activate & scrapy crawl yara_spider