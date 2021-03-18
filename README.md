# Örnek Tye Uygulaması V2 (SchoolOfMath)

Bu seferki Tye senaryosunda aşağıdaki senaryoyu icra edeceğiz. [Kaynak](https://www.packtpub.com/product/adopting-net-5/9781800560567) : Pact Publishing'den Adopting .NET 5: Understand Modern Architectures, migration best practices, and the new features in .NET 5.

![Project_Tye_Senaryo.png](./assets/Project_Tye_Senaryo.png)

- Einstein, gRPC tabanlı bir servis sağlayıcı. İçinde Palindrom sayıları hesap eden bir fonksiyon desteği sunuyor. Servis cache stratejisi için Redis'i kullanacak. 
Cache'te ne mi tutacağız? Daha önceden Palindrome olarak işaretleniş bir sayı varsa bunu kendi adıyla Cache'e alacağız ve 1 saat boyunca orada tutacağız. Aynı sayı tekrar istenirse hesaplanmadan doğrudan cache'den gelecek. Sırf Redis'i senaryoya katalım diye...
Ayrıca bir mesaj kuyruğu sistemini de destekleyecek ki bu noktada RabbitMQ'dan yararlanacağız.

- Evelyne, Bruce ve Madeleine Worker tipinden istemci servisler. _(Onları, başladıktan sonra sürekli talep gönderen servisler olarak düşünelim)_ Belli bir sayıdan başlayarak Eintesein'a talep gönderiyorlar ve gönderikleri sayının Palindrom olup olmadığı bilgisini alıyorlar.

- Robert ise RabbitMQ kuyruğunu dinleyen diğer bir Worker servisimiz.

Amacımız bu senaryoyu Tye destekli olarak inşa edip kolay bir şekilde Kubernetes'e alabilmek. Daha ilkel bir sürüm için [StarCups isimli örneğe](https://github.com/buraksenyurt/tye_sample) de bakabilirsiniz.

_Platform : Windows 10(Sistemde .Net 5, Docker Desktop, kubectl, wsl2 ve tye mevcut)_

## Proje İskeletinin Oluşturulması

Bunun için aşağıdaki adımları icra edelim.

```bash
mkdir SchoolOfMath
cd SchoolOfMath

dotnet new sln

# Einstein isimli gRPC servisinin geliştirilmesi
dotnet new grpc -n Einstein
dotnet sln add Einstein

# Protos klasöründeki greet.proto değiştirilir
# Akabinde servis sınıfı da

# İlk İstemci tarafı oluşturulur
dotnet new worker -n Evelyne
dotnet sln add Evelyne
# Evelyne'nin gRPC servisini kullanabilmesi için gerekli Nuget paketleri eklenir.
cd Evelyne
dotnet add package Grpc.Net.Client
dotnet add package Grpc.Net.ClientFactory
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
# Ayrıca Tye konfigurasyonu için gerekli extension paketi de yüklenir
dotnet add package --prerelease Microsoft.Tye.Extensions.Configuration
cd ..

# Visual Studio 2019 kullanıyorsak Add new gRPC Service Reference(Connected Services kısmından) ile Einstein'daki proto dosyasının fiziki adresi gösterilerek gerekli proxy tipinin üretilmesi kolayca sağlanabilir.

# İkinci Worker servisi ekliyoruz (Bruce)
# Tek fark 1den değil de 10000den başlamasıdır (Burada da Add new gRPC servis yapmayı unutmayalım)
dotnet new worker -n Bruce
dotnet sln add Bruce
cd Bruce
dotnet add package Grpc.Net.Client
dotnet add package Grpc.Net.ClientFactory
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
dotnet add package --prerelease Microsoft.Tye.Extensions.Configuration
cd ..

# Üçüncü Worker servis Madeleine de benzer şekilde eklenir
dotnet new worker -n Madeleine
dotnet sln add Madeleine
cd Madeleine
dotnet add package Grpc.Net.Client
dotnet add package Grpc.Net.ClientFactory
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
dotnet add package --prerelease Microsoft.Tye.Extensions.Configuration
cd ..

# Yukradaki işlemler tamamlandıktan sonra en azından aşağıdaki terminal komutu ile 
# servisleri ayağa kaldırıp loglara bakmakta yarar var
tye run
```

![screenshot_1.png](./assets/screenshot_1.png)

![screenshot_2.png](./assets/screenshot_2.png)

![screenshot_3.png](./assets/screenshot_3.png)

## Redis Desteğinin Eklenmesi

Hem redis hem rabbitmq hizmetlerinin ilave edilmesi hem de kubernetes geçiş hazırlıkları için tye.yaml dosyasını oluşturmalıyız.

```bash
tye init

# tye.yaml dosyasın redis için gerekli ekleri yaptıktan sonra
# einstein (gRPC API servisimiz) cache desteği için gerekli nuget paketleri eklenir
cd einstein
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
cd ..
```

Şu noktada tye run ile çalıştırdığımızda en azından aşığıdaki gibi Redis kullanıldığını görmek lazım.

![screenshot_4.png](./assets/screenshot_4.png)

![screenshot_5.png](./assets/screenshot_5.png)

## RabbitMQ Hizmetinin Eklenmesi

Palindrome sayılar buldukça bunları RabbitMQ'ya mesaj olarak yollayacak bir düzenek ekleyeceğiz. RabbitMQ'da, Redis gibi çalışma zamanında ayakta olması beklenen bir servis. Bu nedenle tye.yaml dosyasında gerekli eklemeler yapılmalı.

Sonrasında Einstein isimli servis uygulamasına rabbitmq paketini ekliyoruz.

```bash
cd Einstein
dotnet add package RabbitMQ.Client
cd ..
```

Kod tarafında RabbitMQ kullanımı için gerekli tipler, GoldenHammer isimli sınıfta yer alıyor. [Kaynak](https://github.com/PacktPublishing/Adopting-.NET-5--Architecture-Migration-Best-Practices-and-New-Features/tree/master/Chapter04/microservicesapp) 
_(God Object tadındaki bir sınıf ama senaryoda kullanmak basit olduğundan işime geldi. Daha iyi bir şekilde düzenlemek lazım)_

Bu noktada yine tye run diye ilerleyip http://localhost:15672 adresine ulaşarak RabbitMQ tarafının işler olduğunu görmekte yarar var.

![screenshot_6.png](./assets/screenshot_6.png)

## AMQP İstemcisinin Eklenmesi (Robert)

_EKLENECEK_

## Kubernetes Hazırlıkları

_EKLENECEK_