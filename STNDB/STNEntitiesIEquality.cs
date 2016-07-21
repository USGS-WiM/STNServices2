//https://blogs.msdn.microsoft.com/ericlippert/2011/02/28/guidelines-and-rules-for-gethashcode/    
//http://www.aaronstannard.com/overriding-equality-in-dotnet/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STNDB
{
    public partial class agency :IEquatable<agency>
    {
        public bool Equals(agency other)
        {
            if (other == null) return false;
            return isEqual(this, other);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as agency);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                //var hashCode = 13;
                //hashCode = (hashCode * 397) ^ MyNum;
                //var myStrHashCode =
                //    !string.IsNullOrEmpty(MyStr) ?
                //        MyStr.GetHashCode() : 0;
                //hashCode = (hashCode * 397) ^ MyStr;
                //hashCode =
                //    (hashCode * 397) ^ Time.GetHashCode();
                return (this.agency_name).GetHashCode();
            }
        }        
        private Boolean isEqual(agency x, agency y) {

            return string.Equals(x.agency_name, y.agency_name, StringComparison.InvariantCultureIgnoreCase) &&
                            (string.Equals(x.address, y.address, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(x.address)) &&
                            (string.Equals(x.city, y.city, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(x.city)) &&
                            (string.Equals(x.state, y.state, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(x.state)) &&
                            (string.Equals(x.zip, y.zip, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(x.zip)) &&
                            (string.Equals(x.phone, y.phone, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrEmpty(x.phone));
        }
    }
    public partial class approval : IEquatable<approval>
    {

        public bool Equals(approval other)
        {
            return this.member_id == other.member_id &&
                        (DateTime.Equals(this.approval_date, other.approval_date) || !this.approval_date.HasValue);
                        
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as approval);
        }
        public override int GetHashCode()
        {
            return (this.member_id + this.approval_date.GetHashCode()).GetHashCode();
        }
    }
    public partial class contact : IEquatable<contact>
    {

        public bool Equals(contact other)
        {
            return string.Equals(this.fname.ToUpper(), other.fname.ToUpper()) &&
                                (string.Equals(this.lname.ToUpper(), other.lname.ToUpper()) || string.IsNullOrEmpty(other.lname)) &&
                                (string.Equals(this.email.ToUpper(), other.email.ToUpper()) || string.IsNullOrEmpty(other.email)) &&
                                (this.phone == other.phone || string.IsNullOrEmpty(other.phone)) &&
                                (this.alt_phone == other.alt_phone || string.IsNullOrEmpty(other.alt_phone));
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as contact);
        }
        public override int GetHashCode()
        {
            return (this.fname + this.lname).GetHashCode();
                                                                            
        }
    }
    public partial class contact_type : IEquatable<contact_type>
    {

        public bool Equals(contact_type other)
        {
            return string.Equals(this.type.ToUpper(), other.type.ToUpper());
                
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as contact_type);
        }
        public override int GetHashCode()
        {
            return this.type.GetHashCode();
        }
    }
    public partial class county : IEquatable<county>
    {
        public bool Equals(county other)
        {
            return string.Equals(this.county_name.ToUpper(), other.county_name.ToUpper()) &&
                                 this.state_id == other.state_id;

        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as county);
        }
        public override int GetHashCode()
        {
            return this.state_id + this.county_name.GetHashCode();
        }
    }
    public partial class data_file : IEquatable<data_file>
    {
        public bool Equals(data_file other)
        {
            return (this.instrument_id == other.instrument_id &&
            (!other.start.HasValue || DateTime.Equals(this.start.Value, other.start.Value)) &&
            (!other.end.HasValue || DateTime.Equals(this.end.Value, other.end.Value)) &&
            (!other.good_start.HasValue || DateTime.Equals(this.good_start.Value, other.good_start.Value)) &&
            (!other.good_end.HasValue || DateTime.Equals(this.good_end.Value, other.good_end.Value)) &&
            (!other.collect_date.HasValue || DateTime.Equals(this.collect_date.Value, other.collect_date.Value)) &&
            (!other.processor_id.HasValue || this.processor_id.Value == other.processor_id.Value || other.processor_id.Value <= 0) &&
            (!other.peak_summary_id.HasValue || this.peak_summary_id.Value == other.peak_summary_id.Value || other.peak_summary_id.Value <= 0) &&
            (!other.approval_id.HasValue || this.approval_id.Value == other.approval_id.Value || other.approval_id.Value <= 0));

        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as data_file);
        }
        //public override int GetHashCode()
        //{
        //    return (this.instrument_id + this.start.GetHashCode() + this.end.GetHashCode() +
        //        this.good_start.GetHashCode() + this.good_end.GetHashCode() + this.collect_date.GetHashCode() +
        //        this.processor_id + this.peak_summary_id + this.approval_id).GetHashCode();

        //}
    }
    public partial class deployment_priority : IEquatable<deployment_priority>
    {
        public bool Equals(deployment_priority other)
        {
            return string.Equals(this.priority_name.ToUpper(), other.priority_name.ToUpper());
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as deployment_priority);
        }
        public override int GetHashCode()
        {
            return this.priority_name.ToUpper().GetHashCode();

        }
    }
    public partial class deployment_type : IEquatable<deployment_type>
    {
        public bool Equals(deployment_type other)
        {
            return string.Equals(this.method.ToUpper(), other.method.ToUpper());

        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as deployment_type);
        }
        public override int GetHashCode()
        {
            return this.method.ToUpper().GetHashCode();
        }
    }
    public partial class event_status : IEquatable<event_status>
    {
        public bool Equals(event_status other)
        {
            return string.Equals(this.status.ToUpper(), other.status.ToUpper());
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as event_status);
        }
        public override int GetHashCode()
        {
            return this.status.ToUpper().GetHashCode();
        }
    }
    public partial class event_type : IEquatable<event_type>
    {
        public bool Equals(event_type other)
        {
            return string.Equals(this.type.ToUpper(), other.type.ToUpper());

        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as event_type);
        }
        public override int GetHashCode()
        {
            return this.type.ToUpper().GetHashCode();

        }
    }
    public partial class events : IEquatable<events>
    {
        public bool Equals(events other)
        {
            return string.Equals(this.event_name, other.event_name) &&
                                (!other.event_start_date.HasValue || DateTime.Equals(this.event_start_date.Value, other.event_start_date.Value)) &&
                                (!other.event_end_date.HasValue || DateTime.Equals(this.event_end_date.Value, other.event_end_date.Value)) &&
                                (this.event_type_id == other.event_type_id || other.event_type_id <= 0 || other.event_type_id == null);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as events);
        }
        public override int GetHashCode()
        {
            return this.event_name.GetHashCode() + this.event_start_date.GetHashCode() + this.event_end_date.GetHashCode() + this.event_type_id.GetHashCode();
        }
    }
    public partial class file : IEquatable<file>
    {
        public bool Equals(file other)
        {
            return  (!other.hwm_id.HasValue || this.hwm_id == other.hwm_id) &&
                    (!other.site_id.HasValue || this.site_id == other.site_id) &&
                    (!other.instrument_id.HasValue || this.instrument_id == other.instrument_id) &&
                    (!other.data_file_id.HasValue || this.data_file_id == other.data_file_id) &&
                    (!other.filetype_id.HasValue || this.filetype_id == other.filetype_id) &&
                    (!other.source_id.HasValue || this.source_id == other.source_id) &&
                    (string.IsNullOrEmpty(other.path) || string.Equals(this.path, other.path, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(other.photo_direction) || string.Equals(this.photo_direction, other.photo_direction,StringComparison.OrdinalIgnoreCase)) &&
                    (!other.latitude_dd.HasValue || this.latitude_dd == other.latitude_dd) &&
                    (!other.longitude_dd.HasValue || this.longitude_dd == other.longitude_dd) &&
                    (!other.file_date.HasValue || DateTime.Equals(this.file_date.Value, other.file_date.Value));

        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as file);
        }
        //public override int GetHashCode()
        //{
        //    return this.hwm_id.GetHashCode() + this.site_id.GetHashCode() + this.instrument_id.GetHashCode() + this.data_file_id.GetHashCode() + this.filetype_id.GetHashCode() + this.source_id.GetHashCode() +
        //        this.path.GetHashCode() + this.file_url.GetHashCode() + this.photo_direction.GetHashCode()+
        //        this.latitude_dd.GetHashCode() + this.longitude_dd.GetHashCode() + this.file_date.GetHashCode();
        //}
    }
    public partial class file_type : IEquatable<file_type>
    {
        public bool Equals(file_type other)
        {
            return string.Equals(this.filetype.ToUpper(), other.filetype.ToUpper());
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as file_type);
        }
        public override int GetHashCode()
        {
            return this.filetype.GetHashCode();
        }
    }
    public partial class horizontal_collect_methods : IEquatable<horizontal_collect_methods>
    {
        public bool Equals(horizontal_collect_methods other)
        {
            return string.Equals(this.hcollect_method.ToUpper(), other.hcollect_method.ToUpper());
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as horizontal_collect_methods);
        }
        public override int GetHashCode()
        {
            return this.hcollect_method.GetHashCode();
        }
    }
    public partial class horizontal_datums : IEquatable<horizontal_datums>
    {
        public bool Equals(horizontal_datums other)
        {
            return string.Equals(this.datum_name.ToUpper(), other.datum_name.ToUpper()) || string.IsNullOrEmpty(other.datum_name) &&
                   string.Equals(this.datum_abbreviation.ToUpper(), other.datum_abbreviation.ToUpper()) || string.IsNullOrEmpty(other.datum_abbreviation);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as horizontal_datums);
        }
        public override int GetHashCode()
        {
            return this.datum_name.GetHashCode() + this.datum_abbreviation.GetHashCode();
        }
    }
    public partial class housing_type : IEquatable<housing_type>
    {
        public bool Equals(housing_type other)
        {
            return string.Equals(this.type_name.ToUpper(), other.type_name.ToUpper());
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as housing_type);
        }
        public override int GetHashCode()
        {
            return this.type_name.GetHashCode();
        }
    }
    public partial class hwm : IEquatable<hwm>
    {
        public bool Equals(hwm other)
        {
            return (this.hwm_type_id == other.hwm_type_id &&
                    this.hwm_quality_id == other.hwm_quality_id &&
                    this.site_id.Value == other.site_id.Value &&
                    (this.event_id == other.event_id || other.event_id <= 0 || other.event_id == null) &&
                    (!this.flag_date.HasValue || DateTime.Equals(this.flag_date.Value, other.flag_date.Value)) &&
                    (string.IsNullOrEmpty(other.waterbody) || string.Equals(this.waterbody, other.waterbody, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(other.bank) || string.Equals(this.bank, other.bank, StringComparison.OrdinalIgnoreCase)) &&
                    (!other.latitude_dd.HasValue || this.latitude_dd.Value == other.latitude_dd.Value) &&
                    (!other.longitude_dd.HasValue||this.longitude_dd.Value == other.longitude_dd.Value) &&
                    (!other.elev_ft.HasValue||this.elev_ft.Value == other.elev_ft.Value) &&
                    (!other.vdatum_id.HasValue|| this.vdatum_id.Value == other.vdatum_id.Value) &&
                    (!other.hdatum_id.HasValue||this.hdatum_id.Value == other.hdatum_id.Value) &&
                    (!other.flag_member_id.HasValue || this.flag_member_id.Value == other.flag_member_id.Value) &&
                    (!other.vcollect_method_id.HasValue||this.vcollect_method_id.Value == other.vcollect_method_id.Value) &&
                    (!other.approval_id.HasValue||this.approval_id.Value == other.approval_id.Value) &&
                    (!other.marker_id.HasValue||this.marker_id.Value == other.marker_id.Value) &&
                    (!other.height_above_gnd.HasValue||this.height_above_gnd.Value == other.height_above_gnd.Value));


        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as hwm);
        }
        public override int GetHashCode()
        {
            return this.hwm_type_id + this.hwm_quality_id + this.site_id.GetHashCode() + this.event_id.GetHashCode() + this.flag_date.GetHashCode() +
                   this.waterbody.GetHashCode()+ this.bank.GetHashCode()+ this.latitude_dd.GetHashCode()+ this.longitude_dd.GetHashCode()+
                   this.elev_ft.GetHashCode()+ this.vdatum_id.GetHashCode()+this.hdatum_id.GetHashCode()+ this.flag_member_id.GetHashCode()+this.vcollect_method_id.GetHashCode() +
                   this.approval_id.GetHashCode()+ this.marker_id.GetHashCode()+ this.height_above_gnd.GetHashCode();
        }
    }
    public partial class hwm_qualities : IEquatable<hwm_qualities>
    {
        public bool Equals(hwm_qualities other)
        {
            return string.Equals(this.hwm_quality, other.hwm_quality, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as hwm_qualities);
        }
        public override int GetHashCode()
        {
            return this.hwm_quality.GetHashCode();
        }
    }
    public partial class hwm_types : IEquatable<hwm_types>
    {
        public bool Equals(hwm_types other)
        {
            return string.Equals(this.hwm_type, other.hwm_type, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as hwm_types);
        }
        public override int GetHashCode()
        {
            return this.hwm_type.GetHashCode();
        }
    }
    public partial class instr_collection_conditions : IEquatable<instr_collection_conditions>
    {
        public bool Equals(instr_collection_conditions other)
        {
            return string.Equals(this.condition, other.condition, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as instr_collection_conditions);
        }
        public override int GetHashCode()
        {
            return this.condition.GetHashCode();
        }
    }
    public partial class instrument : IEquatable<instrument>
    {
        public bool Equals(instrument other)
        {
            var t = this.sensor_type_id.Value == other.sensor_type_id.Value &&
                    (!other.deployment_type_id.HasValue || other.deployment_type_id <= 0|| this.deployment_type_id == other.deployment_type_id ) &&
                    (!other.sensor_brand_id.HasValue || other.sensor_brand_id <= 0 || this.sensor_brand_id == other.sensor_brand_id ) &&
                    (!other.interval.HasValue || other.interval <= 0 || this.interval == other.interval ) &&
                    (!other.site_id.HasValue || other.site_id <= 0 || this.site_id == other.site_id) &&
                    (!other.inst_collection_id.HasValue || other.inst_collection_id <= 0 || this.inst_collection_id == other.inst_collection_id) &&
                    (!other.housing_type_id.HasValue || other.housing_type_id <= 0 || this.housing_type_id == other.housing_type_id) &&
                    (!other.event_id.HasValue ||other.event_id <= 0 || this.event_id == other.event_id ) &&
                    (string.IsNullOrEmpty(other.location_description) || string.Equals(this.location_description, other.location_description,StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(other.serial_number) || string.Equals(this.serial_number, other.serial_number,StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(other.vented) || string.Equals(this.vented, other.vented,StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(other.housing_serial_number) || string.Equals(this.housing_serial_number, other.housing_serial_number,StringComparison.OrdinalIgnoreCase) );

            return t;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as instrument);
        }
        //public override int GetHashCode()
        //{
        //    return sensor_type_id.Value + sensor_brand_id.Value + event_id.Value + site_id.Value + serial_number.GetHashCode();
        //}
    }
    public partial class instrument_status: IEquatable<instrument_status>
    {
        public bool Equals(instrument_status other)
        {
            return this.instrument_id == other.instrument_id &&
                   this.status_type_id == other.status_type_id &&
                   (!other.member_id.HasValue || this.member_id == other.member_id || other.member_id <= 0) &&
                   (string.IsNullOrEmpty(other.time_zone) || string.Equals(this.time_zone, other.time_zone, StringComparison.OrdinalIgnoreCase)) &&
                   (!other.sensor_elevation.HasValue || this.sensor_elevation == other.sensor_elevation) &&
                   (!other.ws_elevation.HasValue || this.ws_elevation == other.ws_elevation) &&
                   (!other.gs_elevation.HasValue || this.gs_elevation == other.gs_elevation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as instrument_status);
        }

        public override int GetHashCode()
        {
            return (this.instrument_id + this.status_type_id).GetHashCode() + this.time_stamp.GetHashCode();
        }

    }
    public partial class landownercontact : IEquatable<landownercontact>
    {
        public bool Equals(landownercontact other)
        {
            return (string.IsNullOrEmpty(other.fname) || string.Equals(this.fname, other.fname, StringComparison.OrdinalIgnoreCase)) &&
                   (string.IsNullOrEmpty(other.lname) || string.Equals(this.lname, other.lname, StringComparison.OrdinalIgnoreCase)) &&
                   (string.IsNullOrEmpty(other.address) || string.Equals(this.address, other.address, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as landownercontact);
        }

        public override int GetHashCode()
        {
            return (this.fname+this.lname).GetHashCode();
        }

    }
    public partial class marker : IEquatable<marker>
    {
        public bool Equals(marker other)
        {
            return string.Equals(this.marker1, other.marker1, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as marker);
        }

        public override int GetHashCode()
        {
            return this.marker1.GetHashCode();
        }

    }
    public partial class member : IEquatable<member>
    {

        public bool Equals(member other)
        {
            return string.Equals(this.username, other.username,StringComparison.OrdinalIgnoreCase) &&
                  (string.Equals(this.fname, other.fname,StringComparison.OrdinalIgnoreCase)) &&
                  (string.Equals(this.lname, other.lname, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as member);
        }

        public override int GetHashCode()
        {
            return (this.username + this.fname + this.lname).GetHashCode();
        }
    }
    public partial class network_name : IEquatable<network_name>
    {
        public bool Equals(network_name other)
        {
            return string.Equals(this.name, other.name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as network_name);
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

    }
    public partial class network_type : IEquatable<network_type>
    {
        public bool Equals(network_type other)
        {
            return string.Equals(this.network_type_name, other.network_type_name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as network_type);
        }

        public override int GetHashCode()
        {
            return this.network_type_name.GetHashCode();
        }

    }
    public partial class objective_point : IEquatable<objective_point>
    {

        public bool Equals(objective_point other)
        {
            return (string.Equals(this.name, other.name, StringComparison.OrdinalIgnoreCase) && 
                (string.IsNullOrEmpty(other.description) || string.Equals(this.description, other.description, StringComparison.OrdinalIgnoreCase)) &&
                (!other.date_established.HasValue || DateTime.Equals(this.date_established, other.date_established)) && 
                this.elev_ft == other.elev_ft) &&
                (!other.date_recovered.HasValue || DateTime.Equals(this.date_recovered, other.date_recovered)) &&
                (this.op_is_destroyed == other.op_is_destroyed) &&
                (string.IsNullOrEmpty(other.op_notes) || string.Equals(this.op_notes, other.op_notes, StringComparison.OrdinalIgnoreCase)) &&
                (this.site_id == other.site_id) &&
                (!other.latitude_dd.HasValue || this.latitude_dd == other.latitude_dd) &&
                (!other.longitude_dd.HasValue || this.longitude_dd == other.longitude_dd) &&
                (!other.vdatum_id.HasValue || other.vdatum_id <= 0 || this.vdatum_id == other.vdatum_id) &&
                (other.hdatum_id <= 0 || !other.hdatum_id.HasValue || this.hdatum_id == other.hdatum_id) &&
                (other.vcollect_method_id <= 0 || !other.vcollect_method_id.HasValue || this.vcollect_method_id == other.vcollect_method_id) &&
                (other.hcollect_method_id <= 0 || !other.hcollect_method_id.HasValue || this.hcollect_method_id == other.hcollect_method_id) &&
                (other.uncertainty <= 0 || !other.uncertainty.HasValue || this.uncertainty == other.uncertainty) &&
                (string.IsNullOrEmpty(other.unquantified) || string.Equals(this.unquantified, other.unquantified, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as objective_point);
        }

        public override int GetHashCode()
        {
            return (this.name + this.description + this.op_type_id.ToString() + this.date_established.ToString() + this.site_id.ToString()).GetHashCode();
        }
    }
    public partial class objective_point_type : IEquatable<objective_point_type>
    {

        public bool Equals(objective_point_type other)
        {
            return (string.Equals(this.op_type, other.op_type, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as objective_point_type);
        }

        public override int GetHashCode()
        {
            return (this.op_type).GetHashCode();
        }
    }
    public partial class op_control_identifier : IEquatable<op_control_identifier>
    {

        public bool Equals(op_control_identifier other)
        {
            return (this.objective_point_id == other.objective_point_id &&
                (string.Equals(this.identifier, other.identifier, StringComparison.OrdinalIgnoreCase)) &&
                (string.Equals(this.identifier_type, other.identifier_type, StringComparison.OrdinalIgnoreCase)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as op_control_identifier);
        }

        public override int GetHashCode()
        {
            return (this.objective_point_id.ToString() + this.identifier + this.identifier_type).GetHashCode();
        }
    }
    public partial class op_measurements : IEquatable<op_measurements>
    {

        public bool Equals(op_measurements other)
        {
            return (this.objective_point_id == other.objective_point_id && this.instrument_status_id == other.instrument_status_id &&                
                this.water_surface == other.water_surface && this.ground_surface == other.ground_surface && this.offset_correction == other.offset_correction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as op_quality);
        }

        public override int GetHashCode()
        {
            return (this.objective_point_id.ToString() + this.instrument_status_id.ToString()).GetHashCode();
        }
    }
    public partial class op_quality : IEquatable<op_quality>
    {

        public bool Equals(op_quality other)
        {
            return (string.IsNullOrEmpty(other.quality) || string.Equals(this.quality, other.quality, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as op_quality);
        }

        public override int GetHashCode()
        {
            return (this.quality).GetHashCode();
        }
    }
    public partial class peak_summary : IEquatable<peak_summary>
    {
        public bool Equals(peak_summary other)
        {
            return (this.member_id == other.member_id) && !other.peak_date.HasValue || (DateTime.Equals(this.peak_date.Value, other.peak_date.Value)) &&
                (other.is_peak_estimated <= 0 || this.is_peak_estimated == other.is_peak_estimated || other.is_peak_estimated <= 0 ) &&
                (other.is_peak_time_estimated <= 0 || this.is_peak_time_estimated == other.is_peak_time_estimated || other.is_peak_time_estimated <= 0) &&
                (!other.peak_stage.HasValue || this.peak_stage == other.peak_stage || other.peak_stage <= 0) &&
                (other.is_peak_stage_estimated <= 0 || this.is_peak_stage_estimated == other.is_peak_stage_estimated || other.is_peak_stage_estimated <= 0) &&
                (!other.peak_discharge.HasValue || this.peak_discharge == other.peak_discharge || other.peak_discharge <= 0) &&
                (other.is_peak_discharge_estimated <= 0 || this.is_peak_discharge_estimated == other.is_peak_discharge_estimated || (other.is_peak_discharge_estimated <= 0)) &&
                (!other.vdatum_id.HasValue || this.vdatum_id == other.vdatum_id || other.vdatum_id <= 0) &&
                (!other.height_above_gnd.HasValue || this.height_above_gnd == other.height_above_gnd || other.height_above_gnd <= 0) &&
                (!other.is_hag_estimated.HasValue || this.is_hag_estimated == other.is_hag_estimated || other.is_hag_estimated <= 0) &&
                (!string.IsNullOrEmpty(other.time_zone) || string.Equals(this.time_zone, other.time_zone)) &&
                (!other.aep.HasValue|| this.aep == other.aep || other.aep <= 0) &&
                (!other.aep_lowci.HasValue || this.aep_lowci.Value == other.aep_lowci || other.aep_lowci <= 0) &&
                (!other.aep_upperci.HasValue || this.aep_upperci == other.aep_upperci || other.aep_upperci <= 0) &&
                (!other.aep_range.HasValue || this.aep_range == other.aep_range || other.aep_range <= 0) &&
                (string.Equals(this.calc_notes, other.calc_notes, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as peak_summary);
        }

        public override int GetHashCode()
        {
            return (this.member_id.Value.ToString() + this.peak_date.Value.ToString() + this.time_zone).GetHashCode();
        }
    }
    public partial class reporting_metrics : IEquatable<reporting_metrics>
    {
        public bool Equals(reporting_metrics other)
        {
            return (this.member_id == other.member_id && DateTime.Equals(this.report_date, other.report_date) && this.event_id == other.event_id && 
                this.state == other.state && this.sw_fieldpers_notacct == other.sw_fieldpers_notacct && this.wq_fieldpers_notacct == other.wq_fieldpers_notacct && 
                this.yest_fieldpers == other.yest_fieldpers && this.tod_fieldpers == other.tod_fieldpers && this.tmw_fieldpers == other.tmw_fieldpers &&
                this.yest_officepers == other.yest_officepers && this.tod_officepers == other.tod_officepers && this.tmw_officepers == other.tmw_officepers &&
                this.gage_visit == other.gage_visit && this.gage_down == other.gage_down && this.tot_discharge_meas == other.tot_discharge_meas && 
                this.plan_discharge_meas == other.plan_discharge_meas && this.plan_indirect_meas == other.plan_indirect_meas && this.rating_extens == other.rating_extens && 
                this.gage_peak_record == other.gage_peak_record && this.plan_rapdepl_gage == other.plan_rapdepl_gage && this.dep_rapdepl_gage == other.dep_rapdepl_gage && 
                this.rec_rapdepl_gage == other.rec_rapdepl_gage && this.lost_rapdepl_gage == other.lost_rapdepl_gage && this.plan_wtrlev_sensor == other.plan_wtrlev_sensor && 
                this.dep_wtrlev_sensor == other.dep_wtrlev_sensor && this.rec_wtrlev_sensor == other.rec_wtrlev_sensor && this.lost_wtrlev_sensor == other.lost_wtrlev_sensor && 
                this.plan_wv_sens == other.plan_wv_sens && this.dep_wv_sens == other.dep_wv_sens && this.rec_wv_sens == other.rec_wv_sens && this.lost_wv_sens == other.lost_wv_sens &&
                this.plan_barometric == other.plan_barometric && this.dep_barometric == other.dep_barometric && this.rec_barometric == other.rec_barometric && 
                this.lost_barometric == other.lost_barometric && this.plan_meteorological == other.plan_meteorological && this.dep_meteorological == other.dep_meteorological && 
                this.rec_meteorological == other.rec_meteorological && this.lost_meteorological == other.lost_meteorological && this.hwm_collected == other.hwm_collected && 
                this.hwm_flagged == other.hwm_flagged && this.qw_discr_samples == other.qw_discr_samples && this.coll_sedsamples == other.coll_sedsamples && this.notes == other.notes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as reporting_metrics);
        }

        public override int GetHashCode()
        {
            return (this.report_date.ToString() + this.event_id + this.state + this.member_id).GetHashCode();
        }
    }
    public partial class role : IEquatable<role>
    {
        public bool Equals(role other)
        {
            return (string.Equals(this.role_name, other.role_name, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(this.role_description, other.role_description, StringComparison.OrdinalIgnoreCase)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as role);
        }

        public override int GetHashCode()
        {
            return (this.role_name + this.role_description).GetHashCode();
        }
    }
    public partial class sensor_brand : IEquatable<sensor_brand>
    {

        public bool Equals(sensor_brand other)
        {
            return (string.Equals(this.brand_name, other.brand_name, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as sensor_brand);
        }

        public override int GetHashCode()
        {
            return (this.brand_name).GetHashCode();
        }
    }
    public partial class sensor_type : IEquatable<sensor_type>
    {

        public bool Equals(sensor_type other)
        {
            return (string.Equals(this.sensor, other.sensor, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as sensor_type);
        }

        public override int GetHashCode()
        {
            return (this.sensor).GetHashCode();
        }
    }
    public partial class site : IEquatable<site>
    {
        public bool Equals(site other)
        {
            return (this.latitude_dd == other.latitude_dd && this.longitude_dd == other.longitude_dd &&
                this.hdatum_id == other.hdatum_id && (string.Equals(this.waterbody, other.waterbody , StringComparison.OrdinalIgnoreCase)) &&
                (string.Equals(this.state, other.state , StringComparison.OrdinalIgnoreCase)) &&
                (string.Equals(this.county, other.county, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(other.address) || string.Equals(this.address, other.address, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(other.zip) || string.Equals(this.zip, other.zip, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(other.other_sid) || string.Equals(this.other_sid, other.other_sid, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(other.usgs_sid) || string.Equals(this.usgs_sid, other.usgs_sid, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(other.noaa_sid) || string.Equals(this.noaa_sid, other.noaa_sid, StringComparison.OrdinalIgnoreCase)) &&
                (other.drainage_area_sqmi == null || this.drainage_area_sqmi == other.drainage_area_sqmi || other.drainage_area_sqmi <= 0) &&
                (other.landownercontact_id == null || this.landownercontact_id == other.landownercontact_id || other.landownercontact_id <= 0) &&
                (other.hcollect_method_id == null || this.hcollect_method_id == other.hcollect_method_id || other.hcollect_method_id <= 0));            
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as site);
        }

        public override int GetHashCode()
        {
            return (this.site_description + this.latitude_dd + this.longitude_dd + this.hdatum_id + this.hcollect_method_id + 
                this.state + this.county + this.waterbody + this.member_id).GetHashCode();
        }
    }
    public partial class site_housing : IEquatable<site_housing>
    {

        public bool Equals(site_housing other)
        {
            return (this.site_id == other.site_id && this.housing_type_id == other.housing_type_id &&
                this.length == other.length && this.amount == other.amount &&
                (string.Equals(this.material, other.material, StringComparison.OrdinalIgnoreCase)) &&
                (string.Equals(this.notes, other.notes, StringComparison.OrdinalIgnoreCase)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as site_housing);
        }

        public override int GetHashCode()
        {
            return (this.site_id + this.amount.Value).GetHashCode();
        }
    }
    public partial class source : IEquatable<source>
    {

        public bool Equals(source other)
        {
            return (string.IsNullOrEmpty(other.source_name) || string.Equals(this.source_name, other.source_name) &&
                (this.agency_id == other.agency_id || other.agency_id <= 0 || other.agency_id == null));               
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as source);
        }

        public override int GetHashCode()
        {
            return (this.source_name + this.agency_id.Value).GetHashCode();
        }
    }
    public partial class state : IEquatable<state>
    {

        public bool Equals(state other)
        {
            return (string.IsNullOrEmpty(other.state_name) || string.Equals(this.state_name, other.state_name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(other.state_abbrev) || string.Equals(this.state_abbrev, other.state_abbrev, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as state);
        }

        public override int GetHashCode()
        {
            return (this.state_name + this.state_abbrev).GetHashCode();
        }
    }
    public partial class status_type : IEquatable<status_type>
    {

        public bool Equals(status_type other)
        {
            return (string.IsNullOrEmpty(other.status) || string.Equals(this.status, other.status, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as status_type);
        }

        public override int GetHashCode()
        {
            return (this.status).GetHashCode();
        }
    }
    public partial class vertical_collect_methods : IEquatable<vertical_collect_methods>
    {

        public bool Equals(vertical_collect_methods other)
        {
            return (string.IsNullOrEmpty(other.vcollect_method) || string.Equals(this.vcollect_method, other.vcollect_method, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as vertical_collect_methods);
        }

        public override int GetHashCode()
        {
            return (this.vcollect_method).GetHashCode();
        }
    }
    public partial class vertical_datums:IEquatable<vertical_datums>
    {

        public bool Equals(vertical_datums other)
        {
            return (string.IsNullOrEmpty(other.datum_name) || string.Equals(this.datum_name, other.datum_name, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(other.datum_abbreviation) || string.Equals(this.datum_abbreviation, other.datum_abbreviation, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as vertical_datums);
        }

        public override int GetHashCode()
        {
            return (this.datum_name + this.datum_abbreviation).GetHashCode();
        }
    }

}
