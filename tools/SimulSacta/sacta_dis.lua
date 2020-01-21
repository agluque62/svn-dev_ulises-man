-- Protocolo SACTA V3.5
-- declare our protocol
sacta35_proto = Proto("sacta35","SACTA V3.5")
-- create a function to dissect it
function sacta35_proto.dissector(buffer,pinfo,tree)
--  Tabla de Tipos de Mensaje
	ttipos = {[0]="Inicio Secuencia",[707]="Peticion Sectorizacion",[710]="Resultado Sectorizacion", [1530]="Presencia",[1632]="Orden de Sectorizacion"}
-- En la lista ONLINE	
	pinfo.cols.protocol = "SACTA35"
	pinfo.cols.info = buffer(0,1):uint() .."-"..buffer(1,1):uint().."-"..buffer(2,2):uint().." => "..buffer(4,1):uint() .."-"..buffer(5,1):uint().."-"..buffer(6,2):uint()..": "..ttipos[buffer(10,2):uint()]
--  Decodificar el HEADER.	
	local subtree    = tree:add(sacta35_proto,buffer(),"Mensaje SACTA V3.5")
	subtree:add(buffer(10,2),"" .. ttipos[buffer(10,2):uint()])
	local st_header  = subtree:add(buffer(0,18),"Cabecera")
	st_header:add(buffer(0,1), "Dominio Origen     : " .. buffer(0,1):uint())
	st_header:add(buffer(1,1), "Centro Origen      : " .. buffer(1,1):uint())
	st_header:add(buffer(2,2), "Usuario Origen     : " .. buffer(2,2):uint())
	st_header:add(buffer(4,1), "Dominio Destino    : " .. buffer(4,1):uint())
	st_header:add(buffer(5,1), "Centro Destino     : " .. buffer(5,1):uint())
	st_header:add(buffer(6,2), "Usuario Destino    : " .. buffer(6,2):uint())	
	st_header:add(buffer(8,2), "Sesion             : " .. buffer(8,2):uint())
	st_header:add(buffer(10,2),"Tipo de Mensaje    : " .. ttipos[buffer(10,2):uint()])
	st_header:add(buffer(12,2),"Opcion y Secuencia : " .. buffer(12,2):uint())
	st_header:add(buffer(14,2),"Longitud           : " .. buffer(14,2):uint())
	st_header:add(buffer(16,4),"Hora               : " .. buffer(16,4):uint())
	local st_data  = subtree:add(buffer(20,buffer:len()-20),"Datos")
	if (buffer(10,2):uint()==0) then
	elseif (buffer(10,2):uint()==707) then
	elseif (buffer(10,2):uint()==710) then
		t_res = {[0]="Sectorizacion RECHAZADA", [1]="Sectorizacion IMPLANTADA"}
		st_data:add(buffer(20,4),"Version  : " .. buffer(20,4):uint())
		st_data:add(buffer(24,1),"Resultado: " .. t_res[buffer(24,1):uint()])
	elseif (buffer(10,2):uint()==1530) then
		st_data:add(buffer(20,2), "Caracteres Procesador   : " .. buffer(20,2):uint())
		st_data:add(buffer(22,10),"Procesador              : " .. buffer(22,10):string())
		st_data:add(buffer(32,2), "Numero Procesador       : " .. buffer(32,2):uint())
		st_data:add(buffer(36,1), "Estado Procesador       : " .. buffer(36,1):uint())
		st_data:add(buffer(37,1), "Subestado Procesador    : " .. buffer(37,1):uint())
		st_data:add(buffer(38,2), "Tiempo de Informe       : " .. buffer(38,2):uint())
		st_data:add(buffer(40,2), "Tiempo Maximo de Informe: " .. buffer(40,2):uint())
	elseif (buffer(10,2):uint()==1632) then
		st_data:add(buffer(20,4),"Version     : " .. buffer(20,4):uint())
--		st_data:add(buffer(24,2),"Version: " .. buffer(24,2):uint())
		local st_asg = st_data:add(buffer(26,2),"Asignaciones: " .. buffer(26,2):uint())
		nsect = buffer(26,2):uint()
		for sec=0,nsect-1 do
			indexsect = 28 + 8*sec
			local st_sector = st_asg:add(buffer(indexsect,8),"Asignacion "..sec)
			st_sector:add(buffer(indexsect+2,4),"Sector  : " .. buffer(indexsect+2,4):string())
			st_sector:add(buffer(indexsect+6,1),"UCS     : " .. buffer(indexsect+6,1):uint())
			st_sector:add(buffer(indexsect+7,1),"Tipo UCS: " .. buffer(indexsect+7,1):uint())
			end
		end	
end
-- load the udp.port table
udp_table = DissectorTable.get("udp.port")
-- register our protocol to handle udp port 19204
udp_table:add(19204,sacta35_proto)
