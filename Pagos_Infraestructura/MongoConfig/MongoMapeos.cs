using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Pagos_Dominio.Entidades;

namespace Pagos_Infraestructura.MongoConfig;

public static class MongoMapeos
{
    private static bool _configurado;

    public static void Configurar()
    {
        if (_configurado) return;

        var dateTimeOffsetSerializer = new DateTimeOffsetComoUtcDateTimeSerializerInterno();
        try
        {
            BsonSerializer.RegisterSerializer(dateTimeOffsetSerializer);
            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset?), new NullableSerializer<DateTimeOffset>(dateTimeOffsetSerializer));
        }
        catch
        {
            // Si otro módulo lo registró antes, no hacemos nada.
        }

        RegistrarSiNoExiste<Pago>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id);
            cm.SetIgnoreExtraElements(true);
        });

        RegistrarSiNoExiste<Promocion>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id);
            cm.SetIgnoreExtraElements(true);
        });

        RegistrarSiNoExiste<MetodoPagoGuardado>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id);
            cm.SetIgnoreExtraElements(true);
        });

        RegistrarSiNoExiste<FacturaDigital>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id);
            cm.SetIgnoreExtraElements(true);
        });

        _configurado = true;
    }

    private sealed class DateTimeOffsetComoUtcDateTimeSerializerInterno : StructSerializerBase<DateTimeOffset>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTimeOffset value)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            context.Writer.WriteDateTime(value.ToUniversalTime().ToUnixTimeMilliseconds());
        }

        public override DateTimeOffset Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            var bsonType = context.Reader.GetCurrentBsonType();

            if (bsonType == BsonType.DateTime)
            {
                var ms = context.Reader.ReadDateTime();
                return DateTimeOffset.FromUnixTimeMilliseconds(ms);
            }

            // Formato histórico del driver: [ticks, offsetMinutes]
            if (bsonType == BsonType.Array)
            {
                context.Reader.ReadStartArray();

                var tipoPrimero = context.Reader.GetCurrentBsonType();
                long ticks = tipoPrimero switch
                {
                    BsonType.Int64 => context.Reader.ReadInt64(),
                    BsonType.Int32 => context.Reader.ReadInt32(),
                    _ => LeerComoLongOSaltar(context)
                };

                var tipoSegundo = context.Reader.GetCurrentBsonType();
                int offsetMin;
                if (tipoSegundo == BsonType.Int32)
                    offsetMin = context.Reader.ReadInt32();
                else if (tipoSegundo == BsonType.Int64)
                    offsetMin = (int)context.Reader.ReadInt64();
                else if (tipoSegundo == BsonType.EndOfDocument)
                    offsetMin = 0;
                else
                {
                    context.Reader.SkipValue();
                    offsetMin = 0;
                }

                while (context.Reader.GetCurrentBsonType() != BsonType.EndOfDocument)
                    context.Reader.SkipValue();

                context.Reader.ReadEndArray();

                // Si son ticks, creamos el DateTimeOffset con offset y lo normalizamos a UTC.
                var dto = new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMin));
                return dto.ToUniversalTime();
            }

            // Variante histórica posible: { Ticks: ..., Offset: ... }
            if (bsonType == BsonType.Document)
            {
                context.Reader.ReadStartDocument();

                long? ticks = null;
                int offsetMin = 0;

                while (true)
                {
                    var tipoCampo = context.Reader.ReadBsonType();
                    if (tipoCampo == BsonType.EndOfDocument) break;

                    var nombre = context.Reader.ReadName();
                    if (string.Equals(nombre, "Ticks", StringComparison.OrdinalIgnoreCase))
                    {
                        ticks = tipoCampo switch
                        {
                            BsonType.Int64 => context.Reader.ReadInt64(),
                            BsonType.Int32 => context.Reader.ReadInt32(),
                            _ => LeerComoLongOSaltarNullable(context)
                        };
                    }
                    else if (string.Equals(nombre, "Offset", StringComparison.OrdinalIgnoreCase))
                    {
                        if (tipoCampo == BsonType.Int32)
                            offsetMin = context.Reader.ReadInt32();
                        else if (tipoCampo == BsonType.Int64)
                            offsetMin = (int)context.Reader.ReadInt64();
                        else
                        {
                            context.Reader.SkipValue();
                            offsetMin = 0;
                        }
                    }
                    else
                    {
                        context.Reader.SkipValue();
                    }
                }

                context.Reader.ReadEndDocument();

                if (ticks.HasValue)
                {
                    var dto = new DateTimeOffset(ticks.Value, TimeSpan.FromMinutes(offsetMin));
                    return dto.ToUniversalTime();
                }

                throw new FormatException("No se pudo deserializar DateTimeOffset desde documento BSON.");
            }

            throw new NotSupportedException($"Tipo BSON '{bsonType}' no soportado para DateTimeOffset.");
        }

        private static long LeerComoLongOSaltar(BsonDeserializationContext context)
        {
            context.Reader.SkipValue();
            return 0L;
        }

        private static long? LeerComoLongOSaltarNullable(BsonDeserializationContext context)
        {
            context.Reader.SkipValue();
            return null;
        }
    }

    private static void RegistrarSiNoExiste<T>(Action<BsonClassMap<T>> configuracion)
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(T))) return;
        BsonClassMap.RegisterClassMap(configuracion);
    }
}
