Скачать:
https://github.com/nanoboard/nanoboard/releases


### Разработчикам (как выпустить обновление 1.х или 2.0 клиента): 
чтобы обновить релиз нужно залить новый зип архив в ветку (master для 1.х и feature/2.0 для 2.0),
этим занимаются скрипты (в master deploy.sh), в feature/2.0 build.sh,
скрипты эти - для юникс-осей (зависимость: zip - sudo apt-get install zip).

#### еще раз: 
всё происходит автоматически, скрипты билдят нужные проекты,
добавляют в зип архив нужные файлы,
делают гит коммит,
делают пуш в ветку.

перед этим не забудьте закоммитить свои изменения которые идут в релиз.

после этого на страничке релиза по ссылке будет автоматически доступна новая версия 1.х / 2.0,
страничку релиза для этого менять не нужно!!!

номер версии (в 1.х) обновляется автоматически скриптом (формат X.Y.Z, инкрементится только Z, Y менять вручную в папке version, X менять не нужно, 1.х пусть остается 1.х),
в 2.0 номера версии нет но есть дата билда по ней идет сверка в клиенте (и выдается уведомление юзеру об обновлении если дата билда в репозитории отличается).

разработчики,  обсуждайте серьезные изменения в пулл-реквестах и старайтесь не пушить напрямую. если ваш пулл реквест игнорят продолжительное время без объяснений - возможно остальные пропали или умерли, тогда пушьте без аппрува. аппрув нужен хотя бы от одного (пока нас трое), то есть чтобы большинство (например 2 из 3 - ты и аппрувер) было бы "за".
