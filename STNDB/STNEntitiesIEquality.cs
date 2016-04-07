﻿//https://blogs.msdn.microsoft.com/ericlippert/2011/02/28/guidelines-and-rules-for-gethashcode/    
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
            (!other.start.HasValue | DateTime.Equals(this.start.Value, other.start.Value)) &&
            (!other.end.HasValue | DateTime.Equals(this.end.Value, other.end.Value)) &&
            (!other.good_start.HasValue | DateTime.Equals(this.good_start.Value, other.good_start.Value)) &&
            (!other.good_end.HasValue | DateTime.Equals(this.good_end.Value, other.good_end.Value)) &&
            (!other.collect_date.HasValue | DateTime.Equals(this.collect_date.Value, other.collect_date.Value)) &&
            (!other.processor_id.HasValue | this.processor_id.Value == other.processor_id.Value || other.processor_id.Value <= 0) &&
            (!other.peak_summary_id.HasValue | this.peak_summary_id.Value == other.peak_summary_id.Value || other.peak_summary_id.Value <= 0) &&
            (!other.approval_id.HasValue | this.approval_id.Value == other.approval_id.Value || other.approval_id.Value <= 0));

        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as data_file);
        }
        public override int GetHashCode()
        {
            return (this.instrument_id + this.start.GetHashCode() + this.end.GetHashCode() +
                this.good_start.GetHashCode() + this.good_end.GetHashCode() + this.collect_date.GetHashCode() +
                this.processor_id + this.peak_summary_id + this.approval_id).GetHashCode();

        }
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
                                (DateTime.Equals(this.event_start_date.Value, other.event_start_date.Value) || !other.event_start_date.HasValue) &&
                                (DateTime.Equals(this.event_end_date.Value, other.event_end_date.Value) || !other.event_end_date.HasValue) &&
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
                    (string.IsNullOrEmpty(other.file_url) ||string.Equals(this.file_url, other.file_url, StringComparison.OrdinalIgnoreCase)) &&
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
        public override int GetHashCode()
        {
            return this.hwm_id.GetHashCode() + this.site_id.GetHashCode() + this.instrument_id.GetHashCode() + this.data_file_id.GetHashCode() + this.filetype_id.GetHashCode() + this.source_id.GetHashCode() +
                this.path.GetHashCode() + this.file_url.GetHashCode() + this.photo_direction.GetHashCode()+
                this.latitude_dd.GetHashCode() + this.longitude_dd.GetHashCode() + this.file_date.GetHashCode();
        }
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
                    DateTime.Equals(this.flag_date.Value, other.flag_date.Value) &&
                    (string.Equals(this.waterbody.ToUpper(), other.waterbody.ToUpper()) || string.IsNullOrEmpty(other.waterbody)) &&
                    (string.Equals(this.bank.ToUpper(), other.bank.ToUpper()) || string.IsNullOrEmpty(other.bank)) &&
                    (this.latitude_dd.Value == other.latitude_dd.Value || other.latitude_dd.Value <= 0 || !other.latitude_dd.HasValue) &&
                    (this.longitude_dd.Value == other.longitude_dd.Value || other.longitude_dd.Value <= 0 || !other.longitude_dd.HasValue) &&
                    (this.elev_ft.Value == other.elev_ft.Value || other.elev_ft.Value <= 0 || !other.elev_ft.HasValue) &&
                    (this.vdatum_id.Value == other.vdatum_id.Value || other.vdatum_id.Value <= 0 || !other.vdatum_id.HasValue) &&
                    (this.hdatum_id.Value == other.hdatum_id.Value || other.hdatum_id.Value <= 0 || !other.hdatum_id.HasValue) &&
                    (this.flag_member_id.Value == other.flag_member_id.Value || other.flag_member_id.Value <= 0 || !other.flag_member_id.HasValue) &&
                    (this.vcollect_method_id.Value == other.vcollect_method_id.Value || other.vcollect_method_id.Value <= 0 || !other.vcollect_method_id.HasValue) &&
                    (this.approval_id.Value == other.approval_id.Value || other.approval_id.Value <= 0 || !other.approval_id.HasValue) &&
                    (this.marker_id.Value == other.marker_id.Value || other.marker_id.Value <= 0 || !other.marker_id.HasValue) &&
                    (this.height_above_gnd.Value == other.height_above_gnd.Value || other.height_above_gnd.Value <= 0 || !other.height_above_gnd.HasValue));


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
