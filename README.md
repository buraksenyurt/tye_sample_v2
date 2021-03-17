# Örnek Tye Uygulaması V2 (SchoolOfMath)

Bu seferki Tye senaryosunda aşağıdaki senaryoyu icra edeceğiz. [Kaynak](https://www.packtpub.com/product/adopting-net-5/9781800560567) : Pact Publishing'den Adopting .NET 5: Understand Modern Architectures, migration best practices, and the new features in .NET 5.

![Project_Tye_Senaryo.png](./assets/Project_Tye_Senaryo.png)

Einstein, gRPC tabanlı bir servis sağlayıcı. İçinde Palindrom sayıları hesap eden bir fonksiyon desteği sunuyor. Servis cache stratejisi için Redis'i kullanacak. Ayrıca bir mesaj kuyruğu sistemini de destekleyecek ki bu noktada RabbitMQ'dan yararlanacağız.

Evelyne, Bruce ve Madeleine Worker tipinden istemci servisler. Belli bir sayıdan başlayarak Eintesein'a talep gönderiyorlar ve gönderikleri sayının Palindrom olup olmadığı bilgisini alıyorlar.

Robert ise RabbitMQ kuyruğunu dinleyen diğer bir Worker servisimiz.

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
```

## 